using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArticleCheck.Core.NLP.Interfaces;
using ArticleCheck.Core.NLP.Models;

namespace ArticleCheck.Core.NLP.TfIdf
{
    public class TfIdfSimilarityScoring : ISimilarityScoring
    {
        /// <summary>
        /// We don't actually need to store Documents in memory, it could be stored in Redis, but for simplicity I left it there
        /// </summary>
        private static Dictionary<int, Document> Documents { get; set; } = new Dictionary<int, Document>();

        private readonly ITextPreprocessor _textPreprocessor;
        private readonly IKeyValueStorage _keyValueStorage;

        /// <summary>
        /// Storing term frequencies in following format, so it will be O(1) complexity on getting results
        /// {
        ///   "tfIdf:token:day": {
        ///       "frequency": 5,
        ///       "documentIds": [ 1, 2, 3 ]
        ///    }
        /// }
        /// </summary>
        private readonly Func<string, string> _tokenContextCacheKey = token => $"tfIdf:token:{token}";

        /// <summary>
        /// Storing tf-idf value for token in document, so it will be O(1) complexity on getting value
        /// {
        ///    "tfIdf:document:1:day": 0.26,
        ///    "tfIdf:document:2:day": 0.13,
        ///    "tfIdf:document:3:day": 0.54
        /// }
        /// </summary>
        private readonly Func<Document, string, string> _tfIdfCacheKey = (document, token) => $"tfIdf:document:{document.Id}:{token}";

        public TfIdfSimilarityScoring(
            ITextPreprocessor textPreprocessor,
            IKeyValueStorage keyValueStorage)
        {
            _textPreprocessor = textPreprocessor;
            _keyValueStorage = keyValueStorage;
        }

        /// <summary>
        /// Initialize Documents base, pre-calculate Token Frequencies and TfIdf vectors
        /// </summary>
        public async Task InitAsync(List<Document> documents)
        {
            Documents = documents.ToDictionary(x => x.Id, x => x);

            // We should calculate token frequencies for all documents first,
            // Because it's required for TfIdf vector calculation
            foreach (var document in documents)
            {
                await SaveTokenFrequenciesAsync(document);
            }

            foreach (var document in documents)
            {
                await SaveTfIdfVectorAsync(document);
            }
        }

        public List<string> Tokenize(Document document)
        {
            return _textPreprocessor.Tokenize(document.Content);
        }


        public async Task<List<SimilarityResult>> GetSimilarityScoresAsync(Document document)
        {
            var scores = new List<SimilarityResult>(Documents.Count);
            var tfIdfContext = GetTfIdfVector(document.Tokens);

            foreach (var documentEntry in Documents)
            {
                var targetDocument = documentEntry.Value;
                var score = await GetSimilarityScoreAsync(tfIdfContext, targetDocument);

                scores.Add(score);
            }

            return scores;
        }

        public Task<SimilarityResult> GetSimilarityScoreAsync(int sourceDocumentId, int targetDocumentId)
        {
            return GetSimilarityScoreAsync(Documents[sourceDocumentId], Documents[targetDocumentId]);
        }

        public Task<SimilarityResult> GetSimilarityScoreAsync(Document sourceDocument, Document targetDocument)
        {
            return GetSimilarityScoreAsync(GetTfIdfVector(sourceDocument.Tokens), targetDocument);
        }

        public Task<SimilarityResult> GetSimilarityScoreAsync(Dictionary<string, double> tfIdfSourceContext, Document targetDocument)
        {
            var tfIdfSourceVector = tfIdfSourceContext.Values.ToList();
            var tfIdfTargetVector = tfIdfSourceContext.Keys.Select(t =>
                {
                    var key = _tfIdfCacheKey(targetDocument, t);

                    return _keyValueStorage.Contains(key)
                        ? _keyValueStorage.Get<double>(key)
                        : 0d;
                })
                .ToList();

            return Task.FromResult(new SimilarityResult
            {
                Id = targetDocument.Id,
                Score = GetCosineSimilarity(tfIdfSourceVector, tfIdfTargetVector)
            });
        }


        #region Tf Idf calculations

        private Dictionary<string, double> GetTfIdfVector(List<string> tokens)
        {
            var terms = tokens.GroupBy(x => x)
                .Select(x => x.Key)
                .ToList();

            // TF (term frequency) = number of times when term occurs / total number of terms
            var termsFrequencies = tokens
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => (double)x.Count() / terms.Count);


            // IDF (inverse document frequency) = log (total number of docs / number of docs where term appeared)
            var inverseDocumentFrequencies = terms
                .ToDictionary(
                    x => x,
                    x =>
                    {
                        var tokenContext = _keyValueStorage.Get<TokenContext>(_tokenContextCacheKey(x))
                                            ?? new TokenContext();
                        var termFrequency = tokenContext.Frequency > 0 ? tokenContext.Frequency : 1;

                        return Math.Log((double)Documents.Count / termFrequency);
                    });

            // TF-IDF = TF * IDF
            return termsFrequencies
                .ToDictionary(x => x.Key,
                    x => x.Value * inverseDocumentFrequencies[x.Key]);
        }

        /// <summary>
        /// Cosine Similarity (d1, d2) =  Dot product(d1, d2) / ||d1|| * ||d2||
        /// Dot product (d1, d2) = d1[0] * d2[0] + d1[1] * d2[1] * … * d1[n] * d2[n]
        /// ||d1|| = square root(d1[0]^2 + d1[1]^2 + ... + d1[n]^2)
        /// ||d2|| = square root(d2[0]^2 + d2[1]^2 + ... + d2[n]^2)
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <returns></returns>
        private double GetCosineSimilarity(List<double> vector1, List<double> vector2)
        {
            if (vector1.Count != vector2.Count)
            {
                throw new InvalidOperationException($"{nameof(GetCosineSimilarity)} doesn't support vectors of different length");
            }

            var dotProduct = vector1.Select((v1, i) => v1 * vector2[i])
                                          .Sum();

            var d1 = Math.Sqrt(vector1.Sum(x => Math.Pow(x, 2)));
            var d2 = Math.Sqrt(vector2.Sum(x => Math.Pow(x, 2)));
            
            const double tolerance = 0.00001;

            return Math.Abs(dotProduct) < tolerance
            ? 0
            : dotProduct / (d1 * d2);
        }


        #endregion


        #region Cache Helpers

        /// <summary>
        /// Calculates Token Frequencies in document and persist it in Cache (for faster access)
        /// This method means that we adding {document} to our similarity model
        /// </summary>
        private async Task SaveTokenFrequenciesAsync(Document document)
        {
            foreach (var token in document.Tokens)
            {
                var key = _tokenContextCacheKey(token);
                var tokenContext = await _keyValueStorage.GetAsync<TokenContext>(key)
                                    ?? new TokenContext();

                if (!tokenContext.DocumentIds.Contains(document.Id))
                {
                    tokenContext.DocumentIds.Add(document.Id);
                }

                await _keyValueStorage.SetAsync(key, tokenContext);
            }
        }

        /// <summary>
        /// Calculates TfIdf vector in document and persist it in Cache (for faster access)
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        private async Task SaveTfIdfVectorAsync(Document document)
        {
            var tfIdfContext = GetTfIdfVector(document.Tokens);

            foreach (var token in document.Tokens)
            {
                var key = _tfIdfCacheKey(document, token);

                await _keyValueStorage.SetAsync(key, tfIdfContext[token]);
            }
        }

        /// <summary>
        /// Returns list of documents which have at least 1 similar term to passed document
        ///
        /// Usually this method is needed for getting list of docs for which we need perform tf-idf recalculations
        /// </summary>
        public List<Document> GetRelatedDocuments(Document document)
        {
            return document.Tokens
                .SelectMany(t =>
                {
                    var tokenContext = _keyValueStorage.Get<TokenContext>(_tokenContextCacheKey(t))
                                        ?? new TokenContext();

                    return tokenContext.DocumentIds;
                })
                // Take unique document ids (because document can appear in multiply tokens)
                .Distinct()
                .Where(x => x != document.Id && Documents.ContainsKey(x))
                .Select(x => Documents[x])
                .ToList();
        }

        #endregion

        #region Adjusting document base

        /// <summary>
        /// When we added item to database, we need to add it to cache model as well
        /// 1. Get all affected tokens and their documentIds where it appeared
        /// 2. Add {documentNew} to cache model
        /// 3. Recalculate TfIdf vector for affected documentIds including current one
        /// 
        /// Method will trigger recalculation of TfIdf vector for all documents that were affected by this add
        /// </summary>
        public async Task AddAsync(Document document)
        {
            var documentsAffected = GetRelatedDocuments(document);

            // Add document to cache model
            await SaveTokenFrequenciesAsync(document);

            documentsAffected.Add(document);
            Documents.Add(document.Id, document);

            // Recalculate TfIdf vector for affected documentIds
            await Task.WhenAll(documentsAffected.Select(SaveTfIdfVectorAsync));
        }

        /// <summary>
        /// When item in database is removed, we need to adjust cache model
        /// 1. Get all affected tokens and their documentIds where it appeared
        /// 2. Remove {document} cache data
        /// 3. Recalculate TfIdf vector for affected documentIds
        /// </summary>
        public async Task RemoveAsync(Document document)
        {
            var documentsAffected = GetRelatedDocuments(document);

            Documents.Remove(document.Id);

            foreach (var token in document.Tokens)
            {
                var key = _tokenContextCacheKey(token);
                var tokenContext = await _keyValueStorage.GetAsync<TokenContext>(key);
                tokenContext.DocumentIds.Remove(document.Id);

                await _keyValueStorage.SetAsync(key, tokenContext);
                await _keyValueStorage.RemoveAsync(_tfIdfCacheKey(document, token));
            }

            await Task.WhenAll(documentsAffected.Select(SaveTfIdfVectorAsync));
        }

        #endregion
    }
}
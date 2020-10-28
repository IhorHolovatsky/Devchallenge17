using System.Collections.Generic;
using System.Threading.Tasks;
using ArticleCheck.Core.NLP.Models;

namespace ArticleCheck.Core.NLP.Interfaces
{
    public interface ISimilarityScoring
    {
        Task InitAsync(List<Document> documents);

        List<string> Tokenize(Document document);
        Task AddAsync(Document document);
        Task RemoveAsync(Document document);

        Task<List<SimilarityResult>> GetSimilarityScoresAsync(Document document);
        Task<SimilarityResult> GetSimilarityScoreAsync(int sourceDocumentId, int targetDocumentId);
    }
}
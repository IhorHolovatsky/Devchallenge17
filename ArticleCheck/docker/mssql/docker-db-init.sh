#wait for the SQL Server to come up
sleep 25s && echo "running set up script" && /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d master -i /setup.sql
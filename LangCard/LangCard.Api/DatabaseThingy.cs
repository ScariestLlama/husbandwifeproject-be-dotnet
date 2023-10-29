using Google.Cloud.Datastore.V1;
using Grpc.Core;
using System.Reflection;

namespace LangCard.Api
{
    public class DatabaseThingy
    {
        private readonly DatastoreDb _db;

        public DatabaseThingy()
        {
            var envVars = Environment.GetEnvironmentVariables();

            
            var project = envVars["DATASTOREDB_PROJECT"].ToString();
            
            var devMode = envVars["DEVMODE"].ToString();

            if(devMode.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                var host = envVars["DATASTOREDB_HOST"].ToString();
                var port = Convert.ToInt32(envVars["DATASTOREDB_PORT"]);

                var dsBuilder = new DatastoreClientBuilder();
                dsBuilder.ChannelCredentials = ChannelCredentials.Insecure;
                dsBuilder.Endpoint = $"{host}:{port}";
                var client = dsBuilder.Build();

                _db = DatastoreDb.Create(project, "", client);
            }
            else
            {
                _db = DatastoreDb.Create(project);
            }

            
        }

        public async Task<List<T>> Select<T>(string from) where T : class, new()
        {
            var things = new List<T>();
            var results = await _db.RunQueryAsync(new Query(from));
            foreach (var result in results.Entities)
            {

                if (result == null)
                {
                    continue;
                }

                var thing = new T();

                Type myType = thing.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

                foreach (PropertyInfo prop in props)
                {
                    if(prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                    {
                        prop.SetValue(thing, result.Key.Path.Last().Name);
                        continue;
                    }

                    if(result.Properties.ContainsKey(prop.Name.ToLower()))
                    {
                        var dbProp = result.Properties[prop.Name.ToLower()];
                        prop.SetValue(thing, dbProp.StringValue);
                    }
                }

                things.Add(thing);
            }

            return things;
        }

        public async Task<string> Insert<T>(string into, T model) where T : class, new()
        {
            var entity = new Entity()
            {
                Key = _db.CreateKeyFactory(into).CreateKey(Guid.NewGuid().ToString())
            };

            Type myType = model.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            foreach (PropertyInfo prop in props)
            {
                if (prop.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                entity[prop.Name.ToLower()] = prop.GetValue(model) as string;
            }

            await _db.InsertAsync(entity);

            return entity.Key.Path.Last().Name;
        }
    }

   
}

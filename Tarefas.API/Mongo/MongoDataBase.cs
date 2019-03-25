using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Linq.Expressions;
using MongoDB.Bson;
using System.Globalization;

namespace Tarefas.API.Mongo
{
    public class MongoDatabase<T> : IDatabase<T> where T : class, new()
    {
        private string _connectionString = ConfigurationManager.AppSettings["MongoDBConn"];
        private string _collectionName;
        private IMongoDatabase _db;

        public IMongoCollection<T> _collection
        {
            get
            {
                return _db.GetCollection<T>(_collectionName);
            }
            set
            {
                _collection = value;
            }
        }

        public IQueryable<T> Query
        {
            get
            {
                return _collection.AsQueryable<T>(new AggregateOptions { AllowDiskUse = true });
            }
            set
            {
                Query = value;
            }
        }

        public MongoDatabase(string connectionString, string collectionName)
        {
            _connectionString = connectionString;
            _collectionName = collectionName;
            MongoClient _server = new MongoClient(_connectionString);
            _db = _server.GetDatabase(MongoUrl.Create(_connectionString).DatabaseName);
        }

        public bool Delete(System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            // Remove the object.
            var result = _collection.DeleteOne(expression, new DeleteOptions() { Collation = Collation.Simple });

            return result.DeletedCount == 1;
        }

        public bool DeleteMany(System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            // Remove the object.
            var result = _collection.DeleteMany(expression, new DeleteOptions() { Collation = Collation.Simple });

            return result.DeletedCount == result.DeletedCount;
        }

        public void DeleteAll()
        {
            _db.DropCollection(typeof(T).Name);
        }

        public T Single(System.Linq.Expressions.Expression<Func<T, bool>> expression)
        {
            return Query.Where(expression).SingleOrDefault();
        }

        public IQueryable<T> All(int page, int pageSize)
        {
            return PagingExtensions.Page(Query, page, pageSize);
        }

        public IQueryable<T> All(Expression<Func<T, bool>> expression, int page, int pageSize)
        {
            return PagingExtensions.Page(Query.Where(expression), page, pageSize);
        }

        public IQueryable<T> FilterByJSON(int ps, int cp, string sc, string sd, string fo)
        {
            IQueryable<T> items;
            BsonDocument filters = new BsonDocument();
            BsonDocument firstFilter = new BsonDocument();
            BsonDocument finalFilter = new BsonDocument();
            BsonDocument sort = new BsonDocument();
            int sortOrder = (sd == "ASC") ? 1 : -1;

            if (fo != null && fo != "{}")
            {
                var objFilter = (JObject)JsonConvert.DeserializeObject(fo);
                var lstFilters = objFilter.Children().Cast<JProperty>().Select(j => new { Name = j.Name, Value = (string)j.Value }).Where(x => x.Value != null && x.Value != "").ToList();

                if (lstFilters.Count > 0)
                {
                    foreach (var f in lstFilters)
                    {
                        BsonDocument filter = new BsonDocument();

                        // Se for booleano testa true/false
                        Boolean boolValue;
                        int intValue;
                        if (Boolean.TryParse(f.Value, out boolValue))
                        {
                            filter.Add(f.Name, boolValue);
                        }
                        else if (Int32.TryParse(f.Value, out intValue))
                        {
                            filter.Add(f.Name, Convert.ToInt32(f.Value));
                        }
                        // Se for string testa com lowerCase e contains
                        else
                        {
                            if (f.Name.Substring(0, 2).ToLower() == "id")
                            {
                                filter.Add(f.Name, f.Value);
                            }
                            else
                            {
                                filter.Add(f.Name, new BsonRegularExpression(".*" + f.Value + ".*", "i"));
                            }
                        }

                        if (firstFilter.Count() == 0)
                        {
                            firstFilter.AddRange(filter);
                        }
                        else
                        {
                            filters.AddRange(filter);
                        }
                    }

                    finalFilter.AddRange(firstFilter);
                    finalFilter.AddRange(new BsonDocument("$and", new BsonArray().Add(filters)));
                }
            }

            if (!String.IsNullOrEmpty(sc))
            {
                sort.Add(sc, sortOrder);
            }

            items = _collection.Find(finalFilter).Sort(sort).Skip(ps * (cp - 1)).Limit(ps).ToList().AsQueryable<T>();

            return items;
        }

        public int Count(string fo = null)
        {
            BsonDocument filters = new BsonDocument();
            BsonDocument firstFilter = new BsonDocument();
            BsonDocument finalFilter = new BsonDocument();
            BsonDocument sort = new BsonDocument();

            if (fo != null && fo != "{}")
            {
                var objFilter = (JObject)JsonConvert.DeserializeObject(fo);
                var lstFilters = objFilter.Children().Cast<JProperty>().Select(j => new { Name = j.Name, Value = (string)j.Value }).Where(x => x.Value != null && x.Value != "").ToList();

                if (lstFilters.Count > 0)
                {
                    foreach (var f in lstFilters)
                    {
                        BsonDocument filter = new BsonDocument();

                        // Se for booleano testa true/false
                        Boolean boolValue;
                        int intValue;
                        if (Boolean.TryParse(f.Value, out boolValue))
                        {
                            filter.Add(f.Name, boolValue);
                        }
                        else if (Int32.TryParse(f.Value, out intValue))
                        {
                            filter.Add(f.Name, Convert.ToInt32(f.Value));
                        }
                        // Se for string testa com lowerCase e contains
                        else
                        {
                            if (f.Name.Substring(0, 2).ToLower() == "id")
                            {
                                filter.Add(f.Name, f.Value);
                            }
                            else
                            {
                                filter.Add(f.Name, new BsonRegularExpression(".*" + f.Value + ".*", "i"));
                            }
                        }

                        if (firstFilter.Count() == 0)
                        {
                            firstFilter.AddRange(filter);
                        }
                        else
                        {
                            filters.AddRange(filter);
                        }
                    }

                    finalFilter.AddRange(firstFilter);
                    finalFilter.AddRange(new BsonDocument("$and", new BsonArray().Add(filters)));

                    return Convert.ToInt32(_collection.CountDocuments(finalFilter));
                }
                else
                {
                    return Convert.ToInt32(_collection.EstimatedDocumentCount());
                }
            }
            else
            {
                return Convert.ToInt32(_collection.EstimatedDocumentCount());
            }
        }

        public bool Add(T item)
        {
            try
            {
                _collection.InsertOne(item);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        public int Add(IEnumerable<T> items)
        {
            int count = 0;

            foreach (T item in items)
            {
                if (Add(item))
                {
                    count++;
                }
            }

            return count;
        }

        public bool Update(string filterField, string filterValue, T newItem)
        {
            try
            {
                var builder = Builders<T>.Filter;
                var filter = builder.Eq(filterField, filterValue);

                _collection.ReplaceOne(filter, newItem, new UpdateOptions { IsUpsert = true });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // REVISAR FUNÇÃO DE UPDATE MANY
        public bool UpdateMany(string filterField, string filterValue, string updateField, dynamic updateValue)
        {
            try
            {
                var builder = Builders<T>.Filter;
                var filter = builder.Eq(filterField, filterValue);
                var update = Builders<T>.Update.Set(updateField, updateValue);

                var result = _collection.UpdateMany(filter, update);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        public UpdateResult UpdateMany(System.Linq.Expressions.Expression<Func<T, bool>> expression, UpdateDefinition<T> updateDefinition)
        {
            try
            {
                UpdateOptions updateOption = new UpdateOptions();
                updateOption.IsUpsert = true;
                updateOption.Collation = Collation.Simple;

                var result = _collection.UpdateMany(expression, updateDefinition, updateOption);

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public BsonDocument foToFilter(string fo = null)
        {
            BsonDocument filters = new BsonDocument();
            BsonDocument firstFilter = new BsonDocument();
            BsonDocument finalFilter = new BsonDocument();

            if (fo != null && fo != "{}")
            {
                var objFilter = (JObject)JsonConvert.DeserializeObject(fo);
                var lstFilters = objFilter.Children().Cast<JProperty>().Select(j => new { Name = j.Name, Value = (string)j.Value }).Where(x => x.Value != null && x.Value != "").ToList();

                if (lstFilters.Count > 0)
                {
                    foreach (var f in lstFilters)
                    {
                        BsonDocument filter = new BsonDocument();

                        // Se for booleano testa true/false
                        Boolean boolValue;
                        int intValue;


                        if (Boolean.TryParse(f.Value, out boolValue))
                        {
                            filter.Add(f.Name, boolValue);
                        }
                        else if (f.Name.Substring(0, 4).ToLower() != "nome" && Int32.TryParse(f.Value, out intValue))
                        {
                            filter.Add(f.Name, Convert.ToInt32(f.Value));
                        }
                        else if (f.Name.Substring(0, 2).ToLower() == "dt")
                        {
                            if (f.Value != "")
                            {
                                //var dtValue =  Convert.ToDateTime(f.Value).ToString("yyyy/MM/dd");
                                var dtIni = DateTime.Parse(f.Value.Substring(0, f.Value.Length - 9), new CultureInfo("en-US", true)).Date;
                                var dtEnd = dtIni.AddDays(1);

                                filter.AddRange(new BsonDocument(f.Name,
                                                    new BsonDocument {{
                                                                            "$gte", dtIni
                                                                        }, {
                                                                            "$lt", dtEnd
                                                                        }
                                                                    }));
                            }
                        }
                        // Se for string testa com lowerCase e contains
                        else
                        {
                            if (f.Name.Substring(0, 2).ToLower() == "id")
                            {
                                filter.Add(f.Name, f.Value);
                            }
                            else
                            {
                                filter.Add(f.Name, new BsonRegularExpression(".*" + f.Value + ".*", "i"));
                            }
                        }

                        if (firstFilter.Count() == 0)
                        {
                            firstFilter.AddRange(filter);
                        }
                        else
                        {
                            filters.AddRange(filter);
                        }
                    }

                    finalFilter.AddRange(firstFilter);
                    finalFilter.AddRange(new BsonDocument("$and", new BsonArray().Add(filters)));
                }
            }
            return finalFilter;
        }


    }
}
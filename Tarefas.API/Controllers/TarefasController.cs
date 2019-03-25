using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using Tarefas.API.Models;
using Tarefas.API.Mongo;

namespace Tarefas.API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("tarefas")]
    public class TarefasController : ApiController
    {
        private string dbConn = ConfigurationManager.AppSettings["MongoDBConn"];
        private string db = ConfigurationManager.AppSettings["GrupoPrazoDB"];
        const string collection = "Tarefas";

        [Route("GetAll")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            MongoDatabase<Tarefa> tarefasDB = new MongoDatabase<Tarefa>(dbConn + db, collection);

            try
            {
                IQueryable<Tarefa> items = tarefasDB.Query;
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(items), Encoding.UTF8, "application/json");

                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(InternalServerError(ex));
            }
        }

        [Route("GetOne")]
        [HttpGet]
        [ResponseType(typeof(Tarefa))]
        public IHttpActionResult Get(string idTarefa)
        {
            MongoDatabase<Tarefa> tarefasDB = new MongoDatabase<Tarefa>(dbConn + db, collection);

            try
            {
                Tarefa tarefa = tarefasDB.Single(x => x.idTarefa == idTarefa);

                if (tarefa == null)
                {
                    return NotFound();
                }

                return Ok(tarefa);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Insert")]
        [HttpPost]
        [ResponseType(typeof(Tarefa))]
        public IHttpActionResult Post(Tarefa tarefa)
        {
            MongoDatabase<Tarefa> tarefasDB = new MongoDatabase<Tarefa>(dbConn + db, collection);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                tarefasDB.Add(tarefa);
                return Ok(tarefa);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Update")]
        [HttpPut]
        [ResponseType(typeof(Tarefa))]
        public IHttpActionResult Put(Tarefa tarefa)
        {
            MongoDatabase<Tarefa> tarefasDB = new MongoDatabase<Tarefa>(dbConn + db, collection);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                tarefasDB.Update("idTarefa", tarefa.idTarefa, tarefa);
                return Ok(tarefa);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Delete")]
        [HttpGet]
        [ResponseType(typeof(Tarefa))]
        public Tarefa Delete(string idTarefa)
        {
            try
            {
                MongoDatabase<Tarefa> tarefasDB = new MongoDatabase<Tarefa>(dbConn + db, collection);

                Tarefa Tarefa = tarefasDB.Single(x => x.idTarefa == idTarefa);

                if (Tarefa == null)
                {
                    return null;
                }

                tarefasDB.Delete(x => x.idTarefa == idTarefa);

                return Tarefa;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

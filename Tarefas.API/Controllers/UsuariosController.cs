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
    [RoutePrefix("usuarios")]
    public class UsuariosController : ApiController
    {
        private string dbConn = ConfigurationManager.AppSettings["MongoDBConn"];
        private string db = ConfigurationManager.AppSettings["GrupoPrazoDB"];
        const string collection = "Usuarios";
        

        [Route("GetAll")]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            MongoDatabase<Usuario> usuariosDB = new MongoDatabase<Usuario>(dbConn + db, collection);

            try
            {
                IQueryable<Usuario> items = usuariosDB.Query;
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(items), Encoding.UTF8, "application/json");

                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(InternalServerError(ex));
            }
        }


        
        [Route("Login")]
        [HttpPost]
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult Login(Usuario usuario)
        {
            try
            {
                var result = ValidaLogin(usuario.Login, usuario.Senha);

                if(result == null)
                {
                    return Unauthorized();
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("GetOne")]
        [HttpGet]
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult Get(string idUsuario)
        {
            MongoDatabase<Usuario> usuariosDB = new MongoDatabase<Usuario>(dbConn + db, collection);

            try
            {
                Usuario usuario = usuariosDB.Single(x => x.idUsuario == idUsuario);

                if (usuario == null)
                {
                    return NotFound();
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Insert")]
        [HttpPost]
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult Post(Usuario usuario)
        {
            MongoDatabase<Usuario> usuariosDB = new MongoDatabase<Usuario>(dbConn + db, collection);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioExistente = ValidaLogin(usuario.Login, usuario.Senha);

                if(usuarioExistente != null)
                {
                    return null;
                }

                usuariosDB.Add(usuario);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Update")]
        [HttpPut]
        [ResponseType(typeof(Usuario))]
        public IHttpActionResult Put(Usuario usuario)
        {
            MongoDatabase<Usuario> usuariosDB = new MongoDatabase<Usuario>(dbConn + db, collection);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                usuariosDB.Update("idUsuario", usuario.idUsuario, usuario);
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Delete")]
        [HttpGet]
        [ResponseType(typeof(Usuario))]
        public Usuario Delete(string idUsuario)
        {
            try
            {
                MongoDatabase<Usuario> usuariosDB = new MongoDatabase<Usuario>(dbConn + db, collection);

                Usuario usuario = usuariosDB.Single(x => x.idUsuario == idUsuario);

                if (usuario == null)
                {
                    return null;
                }

                usuariosDB.Delete(x => x.idUsuario == idUsuario);

                return usuario;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private Usuario ValidaLogin(string login, string senha)
        {
            MongoDatabase<Usuario> usuariosDB = new MongoDatabase<Usuario>(dbConn + db, collection);
            Usuario usuario = usuariosDB.Single(x => x.Login.ToLower() == login.ToLower() && x.Senha.ToLower() == senha.ToLower());

            if(usuario == null)
            {
                return null;
            }

            return usuario;
        }

        [Route("ExisteAdmin")]
        [HttpGet]
        public bool ExisteAdmin()
        {
            try
            {
                MongoDatabase<Usuario> usuariosDB = new MongoDatabase<Usuario>(dbConn + db, collection);
                var admin = ValidaLogin("admin", "admin");

                if (admin != null)
                    return true;
                else
                    return false;

            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}

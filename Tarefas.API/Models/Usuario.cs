using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;

namespace Tarefas.API.Models
{
    public class Usuario
    {
        public ObjectId Id { get; private set; }
        [Key]
        public string idUsuario
        {
            get { return Id.ToString(); }
            set { Id = new ObjectId(value); }
        }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }
        public Permissao TipoPermissao { get; set; }


        public enum Permissao
        {
            Admin = 1,
            Basico = 2
        }
    }
}
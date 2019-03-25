using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Tarefas.API.Models
{
    public class Tarefa
    {
        public ObjectId Id { get; private set; }
        [Key]
        public string idTarefa
        {
            get { return Id.ToString(); }
            set { Id = new ObjectId(value); }
        }
        public string Nome { get; set; }
        public bool Concluida { get; set; }
    }
}
namespace YSA.Core.Entities
{
    public class PaqueteCurso
    {
        public int PaqueteId { get; set; }
        public int CursoId { get; set; }

        public virtual Paquete Paquete { get; set; }
        public virtual Curso Curso { get; set; }
    }
}
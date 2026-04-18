namespace YSA.Core.Entities
{
    public class PaqueteProducto
    {
        public int PaqueteId { get; set; }
        public int ProductoId { get; set; }

        public virtual Paquete Paquete { get; set; }
        public virtual Producto Producto { get; set; }
    }
}
namespace OptiEscalada
{
    public class Elemento
    {
        public string Nombre { get; set; } = string.Empty;
        public int Peso { get; set; }
        public int Calorias { get; set; }

        public Elemento() { }

        public Elemento(string nombre, int peso, int calorias)
        {
            Nombre = nombre;
            Peso = peso;
            Calorias = calorias;
        }
    }

}

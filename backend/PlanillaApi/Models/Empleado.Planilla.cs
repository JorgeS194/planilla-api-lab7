namespace PlanillaApi.Models
{
    public partial class Empleado
    {
        public const decimal TasaSeguroSocial = 0.0975m;
        public const decimal TasaSeguroEducativo = 0.0125m;
     

        public string NombreCompleto
        {
            get { return $"{Nombre} {Apellido}"; }
        }

        public decimal DeduccionSeguroSocial
        {
            get { return Math.Round(SalarioBase * TasaSeguroSocial, 2, MidpointRounding.AwayFromZero); }
        }

        public decimal DeduccionSeguroEducativo
        {
            get { return Math.Round(SalarioBase * TasaSeguroEducativo, 2, MidpointRounding.AwayFromZero); }
        }

        public decimal SalarioNeto
        {
            get { return SalarioBase - DeduccionSeguroSocial - DeduccionSeguroEducativo; }
        }
    }
}

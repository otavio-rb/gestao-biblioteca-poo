namespace BibliotecaDigital.Web.Models;

public sealed class Devolucao
{
    public Devolucao(int emprestimoId, DateTime dataDevolucao, bool houveAtraso)
    {
        EmprestimoId = emprestimoId;
        DataDevolucao = dataDevolucao;
        HouveAtraso = houveAtraso;
    }

    public int Id { get; private set; }

    public int EmprestimoId { get; private set; }

    public DateTime DataDevolucao { get; private set; }

    public bool HouveAtraso { get; private set; }

    public void DefinirId(int id) => Id = id;

    public static Devolucao Reconstruir(int id, int emprestimoId, DateTime dataDevolucao, bool houveAtraso)
        => new(emprestimoId, dataDevolucao, houveAtraso) { Id = id };
}

using System;
using System.Data;
using System.Windows.Forms;
using Nbb;

public class Suvirinimas
{
    public int id;
    public DateTime data;
    public string aktoNr;
    public Suvirintojas suvirintojas;
    public Operatorius operatorius;
    public string errMsg;
    public string strVietosKodas;

    public Suvirinimas(DataRow row)
    {
        id = Convert.ToInt32(row["id"]);
        data = (DateTime)row["aktas_data"];
        strVietosKodas = string.Format("{0}.{1}.{2}.{3}.{4}.{5}",
                row["k1"], row["k2"], row["k3"], row["k4"], row["k5"], row["k6"]);
        aktoNr = row["aktas_Nr"].ToString();
        suvirintojas = new Suvirintojas(row["suvirintojo_vardas"].ToString());
        operatorius = new Operatorius(row["operatoriaus_vardas"].ToString(), Convert.ToInt32(row["operatoriaus_kodas"]));       
    }
}

public struct Suvirintojas
{
    public string vardas;
    public Suvirintojas(string joVardas)
    {
        vardas = joVardas;
    }
}


public struct Operatorius
{
    public string vardas;
    public int kodas;
    public Operatorius(string joVardas, int joKodas)
    {
        vardas = joVardas;
        kodas = joKodas;
    }
}
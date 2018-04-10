using System;
using System.Data;
using System.Windows.Forms;
using Nbb;

public class Suvirinimas
{
    public int id;
    public DateTime data;
    internal Vieta vieta;
    public string aktoNr;
    public Suvirintojas suvirintojas;
    public Operatorius operatorius;
    public KMFilialo kelioMeistras;
    public string begioTipas;
    public int begioTemperatura;
    public int protarpis;
    public string errMsg;
    public string strVietosKodas;

    public Suvirinimas(DataRow row)
    {
        id = Convert.ToInt32(row["id"]);
        data = (DateTime)row["aktas_data"];
        vieta = new Vieta();
        try
        {
            vieta = VietosKodasParser.parseSuvirinimas(new VietosKodas(row));
        }
        catch (InvalidDuomenysException invd)
        {
            errMsg = invd.Message;
        }
        catch (InvalidKodasException invk)
        {
            errMsg = invk.Message;
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
        }
        finally
        {
            // nežinau, kaip pagaminamas Vieta vieta:
            // jeigu tiesiog surašomi laukai į mąsyvą, tai čia užtektų vieta.toString(),
            // o jeigu ten kažkas dar tikrinama, tuomet esant netinkamam vietos kodui, man
            // nepagamintų objekto Vieta ir tas savo ruožtų neduotų vieta.toString();
            this.strVietosKodas = string.Format("{0}{1}.{2}{3}{4}{5}.{6}{7}.{8}{9}.{10}",
                row["k11"], row["k12"],
                row["k21"], row["k22"], row["k23"], row["k24"],
                row["k31"], row["k32"],
                row["k41"], row["k42"],
                row["k51"]);
        }

        aktoNr = row["aktas_Nr"].ToString();
        suvirintojas = new Suvirintojas(row["suvirintojo_vardas"].ToString(), row["suvirintojo_kodas"].ToString(), row["suvirintojo_įmonė"].ToString());
        operatorius = new Operatorius(row["operatoriaus_vardas"].ToString(), Convert.ToInt32(row["operatoriaus_kodas"]));
        kelioMeistras = new KMFilialo(row["kelio_meistro_vardas"].ToString(), row["kelio_meistro_meistrija"].ToString());
        begioTipas = row["bėgio_tipas"].ToString();
        begioTemperatura = Convert.ToInt32(row["salyg_begioTemp"]);
        protarpis = Convert.ToInt32(row["begis_protarpisMm"]);
    }
}

public struct Suvirintojas
{
    public string vardas;
    public string kodas;
    public string imone;
    public Suvirintojas(string joVardas, string joKodas, string joImonė)
    {
        vardas = joVardas;
        kodas = joKodas;
        imone = joImonė;
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


public struct KelioMeistras
{
    public string vardas;
    public string imone;
    public KelioMeistras(string joVardas, string joImonė)
    {
        vardas = joVardas;
        imone = joImonė;
    }
}

public struct KMFilialo
{
    public string vardas;
    public string meistrija;
    public KMFilialo(string joVardas, string joMeistrija)
    {
        vardas = joVardas;
        meistrija = joMeistrija;
    }
}
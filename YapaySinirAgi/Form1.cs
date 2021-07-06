using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using ExcelDataReader;
using ExcelApp = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop;
using System.Collections;
using System.Numerics;
using Rationals;

namespace YapaySinirAgi
{
    public partial class Form1 : Form
    {
        double _ogrenmeKatsayisi = 0.01f;
        double _momentum = 0.02f;

        int _girisHucreSayisi = 8;
        int _gizliKatmanHucreSayisi;
        int _satirSayisi=0;

        List<double> _giris1 = new List<double>();
        List<double> _giris2 = new List<double>();
        List<double> _giris3 = new List<double>();
        List<double> _giris4 = new List<double>();
        List<double> _giris5 = new List<double>();
        List<double> _giris6 = new List<double>();
        List<double> _giris7 = new List<double>();
        List<double> _giris8 = new List<double>();

        List<double> _beklenenDegerler = new List<double>();

        List<double> _gizliKatman = new List<double>();
        List<double> _dentritGirisGizli = new List<double>();
        List<double> _dentritGirisCikis = new List<double>();

        List<double> _ciktiKatmani = new List<double>();

        double _hataDegeri = 0;
        double _dagitilacakHata = 0;
        double _bias = 0;
        double _mape = 0;

        int _satir = 0;
        int _epoch = 0;

        List<double> _agirlikDegisimMiktarlari = new List<double>();
        List<double> _gizliKatmanDagitilacakHataDegerleri = new List<double>();
        List<double> _girisGizliKatmanArasiAgirliklar = new List<double>();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        Random random = new Random();
        private void VerileriOku()
        {
            //Dosyanın okunacağı dizin
            string filePath = @"enerji-verimliliği-veri-seti.xls";

            //Dosyayı okuyacağımı ve gerekli izinlerin ayarlanması.
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

            IExcelDataReader excelReader;

            int sayac = 0;

            //Gönderdiğim dosya xls'mi xlsx formatında mı kontrol ediliyor.
            if (Path.GetExtension(filePath).ToUpper() == ".XLS")
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            else
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }

            //Veriler okunmaya başlıyor.
            while (excelReader.Read())
            {
                if (sayac == 0)
                {
                    _satir = random.Next(2, _satirSayisi);
                }

                sayac++;
                //ilk satır başlık olduğu için 2.satırdan okumaya başlıyorum.
                if (sayac == _satir)
                {
                    _giris1.Add(excelReader.GetDouble(0));
                    _giris2.Add(excelReader.GetDouble(1));
                    _giris3.Add(excelReader.GetDouble(2));
                    _giris4.Add(excelReader.GetDouble(3));
                    _giris5.Add(excelReader.GetDouble(4));
                    _giris6.Add(excelReader.GetDouble(5));
                    _giris7.Add(excelReader.GetDouble(6));
                    _giris8.Add(excelReader.GetDouble(7));
                    _beklenenDegerler.Add(excelReader.GetDouble(8));
                    break;
                }

            }
            excelReader.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _gizliKatmanHucreSayisi = Convert.ToInt32(textBox1.Text);

            while (_epoch < 100)
            {
                if (_epoch > 0)
                {
                    _giris1.Clear();
                    _giris2.Clear();
                    _giris3.Clear();
                    _giris4.Clear();
                    _giris5.Clear();
                    _giris6.Clear();
                    _giris7.Clear();
                    _giris8.Clear();
                    _beklenenDegerler.Clear();
                }

                EgitimVeriSeti();

                _giris1 = Olceklendir(_giris1);
                _giris2 = Olceklendir(_giris2);
                _giris3 = Olceklendir(_giris3);
                _giris4 = Olceklendir(_giris4);
                _giris5 = Olceklendir(_giris5);
                _giris6 = Olceklendir(_giris6);
                _giris7 = Olceklendir(_giris7);
                _giris8 = Olceklendir(_giris8);
                _beklenenDegerler = Olceklendir(_beklenenDegerler);

                if (_epoch == 0)
                {
                    GirisGizliArasiDentritUret();
                    GizliCiktiArasiDentritUret();
                }

                Egitim();

                _epoch++;

            }
        }

        private void EgitimVeriSeti()
        {
            toplamVeriSayisi();
            //Verinin %70'i
            _satirSayisi = Convert.ToInt32(_satirSayisi * (7 / 10f));

            for (int i = 0; i < _satirSayisi; i++)//Veri setinin %70'i
            {
                VerileriOku();
            }
        }

        private List<double> Olceklendir(List<double> girisler)
        {
            double min = 0, max = 0;
            for (int i = 0; i < girisler.Count; i++)
            {

                if (i == 0)
                {
                    min = girisler[i];
                    max = girisler[i];
                }

                if (girisler[i] < min)
                {
                    min = girisler[i];
                }

                if (girisler[i] > max)
                {
                    max = girisler[i];
                }
            }

            for (int i = 0; i < girisler.Count; i++)
            {
                girisler[i] = (girisler[i] - min) / (max - min);
            }
            return girisler;
        }

        private double Random()
        {
            double sayi = random.NextDouble() * (1 - 0) + 0;
            return Math.Round(sayi, 5);
        }


        private void GirisGizliArasiDentritUret()
        {
            for (int i = 0; i < _girisHucreSayisi * _gizliKatmanHucreSayisi; i++)
            {
                _dentritGirisGizli.Add(Random());
            }
        }

        List<double> _hataDegerleri = new List<double>();
        List<int> _iterasyonSayisi = new List<int>();

        private void Egitim()
        {
            double total;
            _hataDegeri = 0;
            _mape = 0;

            for (int i = 0; i < _giris1.Count; i++)
            {
                total = ToplamFonksiyonu(i);

                total = 0;
                for (int j = 0; j < _gizliKatman.Count; j++)
                {
                    total += _gizliKatman[j] * _dentritGirisCikis[j];
                }

                _ciktiKatmani.Add(SigmoidFonksiyonu(total));

                MSE(i);
                _hataDegerleri.Add(_hataDegeri);

                Mape(i);
                _iterasyonSayisi.Add(i);


                if (_mape > 3)
                {
                    //ÇALIŞMAYI DURDUR.
                    Goster();
                    _epoch = 100;
                    break;
                }

                CiktiKatmaniDagitalacakHataHesapla();

                AgirlikDegisimMiktariHesapla(i);

                AraKatmanDagitalacakHataHesapla();

                GizliCiktiAgirlikGuncelle();

                GirisGizliAgirlikHesabi(i);

                //Yeni Ağırlıkların Hesaplanması
                DentritAgirlikDegisimi();

                _gizliKatman.Clear();
                _agirlikDegisimMiktarlari.Clear();
                _gizliKatmanDagitilacakHataDegerleri.Clear();
                _girisGizliKatmanArasiAgirliklar.Clear();

            }
        }

        private void DentritAgirlikDegisimi()
        {
            for (int j = 0; j < _dentritGirisGizli.Count; j++)
            {
                _dentritGirisGizli[j] = _dentritGirisGizli[j] + _girisGizliKatmanArasiAgirliklar[j];
            }
        }

        private void GirisGizliAgirlikHesabi(int i)
        {
            for (int j = 0; j < _gizliKatmanDagitilacakHataDegerleri.Count; j++)
            {
                _girisGizliKatmanArasiAgirliklar.Add(-1 * _gizliKatmanDagitilacakHataDegerleri[j] * _ogrenmeKatsayisi * _giris1[i] + _momentum * i);
                _girisGizliKatmanArasiAgirliklar.Add(-1 * _gizliKatmanDagitilacakHataDegerleri[j] * _ogrenmeKatsayisi * _giris2[i] + _momentum * i);
                _girisGizliKatmanArasiAgirliklar.Add(-1 * _gizliKatmanDagitilacakHataDegerleri[j] * _ogrenmeKatsayisi * _giris3[i] + _momentum * i);
                _girisGizliKatmanArasiAgirliklar.Add(-1 * _gizliKatmanDagitilacakHataDegerleri[j] * _ogrenmeKatsayisi * _giris4[i] + _momentum * i);
                _girisGizliKatmanArasiAgirliklar.Add(-1 * _gizliKatmanDagitilacakHataDegerleri[j] * _ogrenmeKatsayisi * _giris5[i] + _momentum * i);
                _girisGizliKatmanArasiAgirliklar.Add(-1 * _gizliKatmanDagitilacakHataDegerleri[j] * _ogrenmeKatsayisi * _giris6[i] + _momentum * i);
                _girisGizliKatmanArasiAgirliklar.Add(-1 * _gizliKatmanDagitilacakHataDegerleri[j] * _ogrenmeKatsayisi * _giris7[i] + _momentum * i);
                _girisGizliKatmanArasiAgirliklar.Add(-1 * _gizliKatmanDagitilacakHataDegerleri[j] * _ogrenmeKatsayisi * _giris8[i] + _momentum * i);
            }
        }

        private void GizliCiktiAgirlikGuncelle()
        {
            for (int j = 0; j < _gizliKatman.Count; j++)
            {
                _dentritGirisCikis[j] = _dentritGirisCikis[j] - _gizliKatmanDagitilacakHataDegerleri[j];
            }
        }

        private void AraKatmanDagitalacakHataHesapla()
        {
            for (int j = 0; j < _gizliKatman.Count; j++)
            {
                _gizliKatmanDagitilacakHataDegerleri.Add(_gizliKatman[j] * ((1 - _gizliKatman[j]) * (_dagitilacakHata * _dentritGirisCikis[j])));
            }
        }

        private void AgirlikDegisimMiktariHesapla(int i)
        {
            for (int j = 0; j < _gizliKatman.Count; j++)
            {
                _agirlikDegisimMiktarlari.Add(_dagitilacakHata * _ogrenmeKatsayisi * _gizliKatman[j] + _momentum * i);
            }
        }

        private void CiktiKatmaniDagitalacakHataHesapla()
        {
            _dagitilacakHata = _ciktiKatmani[_ciktiKatmani.Count - 1] * (1 - _ciktiKatmani[_ciktiKatmani.Count - 1]) * _hataDegeri;
        }

        private void Mape(int i)
        {
            _mape += Math.Abs(_beklenenDegerler[i] - _ciktiKatmani[_ciktiKatmani.Count - 1]) / _beklenenDegerler[i];
            _mape = (_mape / _giris1.Count) * 100;
        }

        private void MSE(int i)
        {
            _hataDegeri = Math.Pow((_beklenenDegerler[i] - _ciktiKatmani[_ciktiKatmani.Count - 1]), 2) / _giris1.Count;
        }

        private double ToplamFonksiyonu(int i)
        {
            double toplam = 0;
            for (int j = 0; j < _dentritGirisGizli.Count; j += 8)
            {
                //ARA KATMAN TOPLAM FONKSİYONU                   
                toplam += _giris1[i] * _dentritGirisGizli[j];
                toplam += _giris2[i] * _dentritGirisGizli[j + 1];
                toplam += _giris3[i] * _dentritGirisGizli[j + 2];
                toplam += _giris4[i] * _dentritGirisGizli[j + 3];
                toplam += _giris5[i] * _dentritGirisGizli[j + 4];
                toplam += _giris6[i] * _dentritGirisGizli[j + 5];
                toplam += _giris7[i] * _dentritGirisGizli[j + 6];
                toplam += _giris8[i] * _dentritGirisGizli[j + 7];
                _bias = Random();
                toplam += _bias;

                _gizliKatman.Add(SigmoidFonksiyonu(toplam));
                toplam = 0;
            }

            return toplam;
        }

        private void Goster()
        {
            for (int i = 0; i < _iterasyonSayisi.Count; i++)
            {
                chart1.Series["Hata Değeri - İterasyon Sayısı"].Points.AddXY(_iterasyonSayisi[i], _hataDegerleri[i]);
            }
        }


        private double SigmoidFonksiyonu(double total)
        {
            double Edegeri = Math.Truncate(Math.E * 100) / 100;
            return (1 / (1 + Math.Exp(-total)));
        }

        private void GizliCiktiArasiDentritUret()
        {
            for (int i = 0; i < _gizliKatmanHucreSayisi; i++)
            {
                _dentritGirisCikis.Add(Random());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Goster();
        }

        private void toplamVeriSayisi()
        {
            //Dosyanın Adresi
            string filePath = @"enerji-verimliliği-veri-seti.xls";

            //Dosya İzinleri
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read);

            IExcelDataReader excelReader;

            //Dosya Excel Mi Değil mi?
            if (Path.GetExtension(filePath).ToUpper() == ".XLS")
            {
                excelReader = ExcelReaderFactory.CreateBinaryReader(stream);
            }
            else
            {
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);
            }

            //Verilerin Okunması
            while (excelReader.Read())
            {
                _satirSayisi++;
            }
            excelReader.Close();
        }
    }
}


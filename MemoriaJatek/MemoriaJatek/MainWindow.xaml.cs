using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MemoriaJatek
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		byte meret = 4;

		byte[,] matrix; //Számok táblázatának tárolása
		SolidColorBrush[] szinek = new SolidColorBrush[] //színek kirajzolásához
			{
			 new SolidColorBrush(Colors.DeepSkyBlue),
			 new SolidColorBrush(Colors.Red),
			 new SolidColorBrush(Colors.SpringGreen),
			 new SolidColorBrush(Colors.Magenta),
			 new SolidColorBrush(Colors.Gold),
			 new SolidColorBrush(Colors.Cyan),
			 new SolidColorBrush(Colors.Navy),
			 new SolidColorBrush(Colors.Tan),
			 new SolidColorBrush(Colors.White),
			 new SolidColorBrush(Colors.Black),
			 new SolidColorBrush(Colors.LightSalmon),
			 new SolidColorBrush(Colors.Olive),
			 new SolidColorBrush(Colors.Indigo),
			 new SolidColorBrush(Colors.Brown),
			 new SolidColorBrush(Colors.Gray),
			 new SolidColorBrush(Colors.DarkKhaki),
			 new SolidColorBrush(Colors.Green),
			 new SolidColorBrush(Colors.Orchid)
			};

		Button aktualis1 = null, aktualis2 = null; // A párok eltárolása

		DispatcherTimer idozito = new DispatcherTimer(); //időzítő létrehozás

		string jatekos;
		uint lepesszam;
		DispatcherTimer stopper = new DispatcherTimer(); //stopper létrehozás eltelt idő mutatására
		DateTime aktualisIdo;

		int megoldott = 0;
		public MainWindow()
		{
			InitializeComponent();
			matrix = new byte[meret, meret]; //mátrixnak helyfoglalás
			idozito.Interval = TimeSpan.FromSeconds(0.5); //fél másodpercenként mér
			idozito.Tick += Elrejt; //amikor lejár a 0.5mp elrejti a lapokat
			stopper.Interval = TimeSpan.FromSeconds(0.5);
			stopper.Tick += Eltelt;
			try
			{
				foreach (var item in File.ReadAllLines("highscore.txt"))
				{
					string[] resz = item.Split(';');
					string szoveg = resz[0] + " lépésszám: " + resz[1] + " idő: " + resz[2] + " táblaméret: " + resz[3];
					highscore.Items.Add(szoveg);
				}
			}
			catch (Exception)
			{
				FileStream fs = new FileStream("highscore.txt",FileMode.Create);
			}
			
		}

		private void jatekInditasa_Click(object sender, RoutedEventArgs e)
		{
			//Grid sor oszlop törlés
			tabla.RowDefinitions.Clear();
			tabla.ColumnDefinitions.Clear();

			jatekos = nevMezo.Text;
			jatekosSzoveg.Content = jatekos;
			lepesszam = 0;
			lepesekSzoveg.Content = lepesszam;
			megoldott = 0;

			//Dinamikus colum, row megadás:
			for (int i = 0; i < meret; i++)
			{
				tabla.RowDefinitions.Add(new RowDefinition());
				tabla.ColumnDefinitions.Add(new ColumnDefinition());
			}
			Random r = new Random();
			List<byte> szamok = new List<byte>();
			//számeltárolás mátrixhoz
			//2x felvenni a számokat:
			for (byte i = 0; i < Math.Pow(meret, 2) / 2; i++)
			{
				szamok.Add(i);
				szamok.Add(i);
			}
			//sorfolytonos mátrixolvasás:
			for (int i = 0; i < meret; i++)
			{
				for (int j = 0; j < meret; j++)
				{
					int index = r.Next(0, szamok.Count);//a fentebbi lista elemindexének kiválasztása
					matrix[i, j] = szamok[index];//a kiválaszott elem mátrixba helyezése
					szamok.RemoveAt(index);//ne legyen mégegyszer az elem
					Button gomb = new Button(); //A gridbe kerülő gombgenerálás
					gomb.Content = "?";
					//int aktualis = matrix[i, j];
					//gomb.Background = szinek[aktualis]; //gombszín tesztelésre

					//10px -es margó beállítása:
					Thickness margo = new Thickness(10, 10, 10, 10);
					gomb.Margin = margo;

					gomb.Click += gombFelfed_Click;
					//vagy :gomb.Click += new RoutedEventHandler(gombFelfed_Click);

					gomb.Name = "b" + i + "_" + j; //Gombelnevezés, minden gomb egyedi pl.: b0_0 = az első gomb

					Grid.SetRow(gomb, i); //gomb sor és oszlopmegadása a gridben
					Grid.SetColumn(gomb, j);
					tabla.Children.Add(gomb); //A gridhez adjuk a gombokat
				}
			}
			aktualisIdo = DateTime.Now;
			stopper.Start();
		}

		private void gombFelfed_Click(object sender, RoutedEventArgs e)
		{
			Button gomb = sender as Button; //megkeresi azt a gombot, amelyre kattintottunk
											//MessageBox.Show(gomb.Name); //melyik gombra kattintottunk tesztje

			//A gomb sor és oszlopszámának kiszedése (index):
			byte sor = Convert.ToByte(gomb.Name.Substring(1, 1));
			byte oszlop = Convert.ToByte(gomb.Name.Substring(3, 1));
			int elem = matrix[sor, oszlop]; //mátrixból kivesszük az elemet
			gomb.Background = szinek[elem];
			gomb.Content = ""; // ha a gomb fel lett fedve ne legyen ? 

			if (aktualis1 == null) // első elem a párból
			{
				aktualis1 = gomb; //első gomb a most kattintott lesz
				aktualis1.IsHitTestVisible = false; //A gombra kattintást tiltja le
			}
			else if (gomb.Background == aktualis1.Background) //két gomb házttere egyezik = 1 pár
			{
				gomb.IsHitTestVisible = false;
				aktualis1 = null; //A pár első elemét lenullázzuk a következő pár kereséséhez
				lepesszam++;
				lepesekSzoveg.Content = lepesszam;
				megoldott++;
				if (megoldott==Math.Pow(meret,2)/2)
				{
					stopper.Stop();
					MessageBox.Show("Nyertél!");
					FileStream fs = new FileStream("highscore.txt",FileMode.Append);
					StreamWriter sw = new StreamWriter(fs);
					sw.WriteLine(jatekos+";"+lepesszam+";"+elteltSzoveg.Content+";"+meret+"x"+meret);
					sw.Close();
					fs.Close();
					//Miután megnyeri a játékot alapértelmezettre állítja az időt, lépést, és méretet újra meg kell adni.
					//Az eredményeket újra betöltjük
					meretLista.SelectedItem = -1;
					lepesekSzoveg.Content = "";
					elteltSzoveg.Content = "";
					tabla.Children.Clear();
					highscore.Items.Clear();
					foreach (var item in File.ReadAllLines("highscore.txt"))
					{
						string[] resz = item.Split(';');
						string szoveg = resz[0] + " lépésszám: " + resz[1] + " idő: " + resz[2] + " táblaméret: " + resz[3];
						highscore.Items.Add(szoveg);
					}
				}
			}
			else
			{
				aktualis2 = gomb; //mindkét gombhoz hozzáférhetünk az Elrejt eseménybe
				ablak.IsHitTestVisible = false; //az ablakban nincs kattintás engedélyezve
				lepesszam++;
				lepesekSzoveg.Content = lepesszam;
				idozito.Start(); //időzítő indítás
			}
		}

		private void meretLista_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			meret = Convert.ToByte(meretLista.SelectedIndex * 2 + 2); //a kiválasztott indexnél: 0*2+2 = 2x2-es mező létrehozás
			matrix = new byte[meret, meret]; //mátrixnak helyfoglalás a dinamikusság miatt
		}

		private void nevMezo_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (nevMezo.Text != "")
				jatekInditasa.IsEnabled = true;
			else
				jatekInditasa.IsEnabled = false;
		}

		private void Elrejt(object sender, EventArgs e) //eseménykezelő az időzítőhöz
		{
			//A gombok alapértelmezetté tétele: 
			aktualis1.IsHitTestVisible = true;
			aktualis1.ClearValue(BackgroundProperty); //a gomb alapértelmezett háttere leaz újra(szürke)
			aktualis1.Content = "?";
			aktualis1 = null;
			aktualis2.IsHitTestVisible = true;
			aktualis2.ClearValue(BackgroundProperty); //a gomb alapértelmezett háttere leaz újra(szürke)
			aktualis2.Content = "?";
			aktualis2 = null;
			ablak.IsHitTestVisible = true;
			idozito.Stop(); //időzítő leállítása
		}

		private void Eltelt(object sender, EventArgs e) //eseménykezelő a stopperhez
		{
			DateTime most = DateTime.Now; //aktuális idő
			TimeSpan eltelt = most.Subtract(aktualisIdo); 
			string elteltIdo = eltelt.Minutes.ToString("D2") + ":" + eltelt.Seconds.ToString("D2"); //Időzítő, D2=Vezető nullák
			elteltSzoveg.Content = elteltIdo;
		}
	}
}

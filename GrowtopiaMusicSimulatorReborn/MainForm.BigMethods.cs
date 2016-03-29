﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;

namespace GrowtopiaMusicSimulatorReborn
{
	public partial class MainForm : Form
	{



		public static bool checkFileExistance(){
			bool gone = false;
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/playButton.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/stopButton.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/Grid.bmp"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/piano.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/pianoFlat.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/pianoSharp.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/bass.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/bassFlat.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/bassSharp.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/drum.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/loadButton.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/saveButton.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/yellowPlayButton.png"))) {
				gone = true;
			}
			if (!File.Exists ((Directory.GetCurrentDirectory () + "/Images/_credits.txt"))) {
				MessageBox.Show ("I don't appreciate you not appreciating.\nSomehow, I doubt you moved the credits file to your desktop so you can look\nat it everyday.");
				gone = true;
			}
			return gone;
		}



		void checkUI(MouseEventArgs e){
			if (e.X<32){
				// When you press play button.
				if (!playing) {
					playing = true;
					playThread = new Thread (new ParameterizedThreadStart(playMusic));
					playThread.Start (0);
				} else {
					playThread.Abort ();
					playing = false;
				}
				needRedraw = true;
				//playMusic();
			}else{
				// When you press note cange button.
				if (e.X < 64) {
					if (noteValue == 7) {
						noteValue = 0;
					} else {
						noteValue++;
					}
					needRedraw = true;
				} else {
					// When you press save button
					if (e.X < 96) {
						save ();
					} else if (e.X < 128) {
						// When you press load button
						OpenFileDialog ofd = new OpenFileDialog ();
						ofd.ShowDialog ();
						FileStream fs = new FileStream (ofd.FileName, FileMode.Open);
						songPlace.maparray = customLoadMapFromFile (ref fs).Item4;
						fs.Dispose ();
						needRedraw = true;
						if (showConfirmation) {
							MessageBox.Show ("Loadedededed.");
						}
					} else if (e.X < 160) {
						// Left button
						// Gotta protect morons from themselves.
						if (pageNumber == 0) {
							return;
						}
						pageNumber--;
						needRedraw = true;
					} else if (e.X < 192) {
						// right button
						// Morons...must protect...
						if (pageNumber == 15) {
							return;
						}
						pageNumber++;
						needRedraw = true;
					} else if (e.X<224) {
						if (!playing) {
							playing = true;
							playThread = new Thread (new ParameterizedThreadStart(playMusic));
							playThread.Start (pageNumber*25);
						} else {
							playThread.Abort ();
							playing = false;
						}
						needRedraw = true;
					}else if (e.X<768){
						MessageBox.Show ("Programming - MyLegGuy\nOriginal theme - SumRndmDde\nBPM formula - y3ll0\nMatching sounds to notes - HonestyCow\n\nThis couldn't be possible\nwithout these people.");
					}else if (e.X>800){
						PopBPM pbpm = new PopBPM(reverseBPMformula(OptionHolder.noteWait));
						pbpm.ShowDialog();
						if (pbpm.numericUpDown1.Value < 20 || pbpm.numericUpDown1.Value > 200) {
							MessageBox.Show ("Yo son.\nGrowtopia don't support dat BPM.\nBut you can still use it here.");
						}
						OptionHolder.noteWait=(short)(bpmFormula(Convert.ToInt32(pbpm.numericUpDown1.Value)));
						pbpm.Dispose();
					}
				}
			}
		}

		public int bpmFormula(int re){
			return 60000 / (4 * re);
		}

		public int reverseBPMformula(int re){
			return 15000/re;
		}

		// It came back to haunt me. Making it so I can have a maximum of 255x255 map. I had to write this custom method now.

		public static Tuple<int,int,int,int[][,]> customLoadMapFromFile(ref FileStream file){
			int mapversion=file.ReadByte();
			int mapWidth=file.ReadByte();
			mapWidth = 400;
			int mapHeight=file.ReadByte();
			mapHeight = 14;
			byte past = 255;
			byte present = 254;
			byte rollValue = 55;
			bool rolling = false;
			int rollAmount = 0;
			int layers = file.ReadByte();
			layers = 1;
			int[][,] workMap = new int[layers][,];
			for (int i = 0; i < layers; i++) {
				workMap [i] = new int[401, 15];
			}
			//Debug.Print(mapversion.ToString()+";"+mapWidth.ToString()+";"+mapHeight.ToString()+".");
			for (int i = 0; i < layers; i++) {
				for (int y = 0; y < mapHeight; y++) {
					for (int x = 0; x < mapWidth; x++) {
						if (!rolling) {
							if (past == present) {
								// Checked here for good reasons.
								rolling = true;
								rollValue = present;
								rollAmount = file.ReadByte ();
								//Debug.Print("Starting roll with value: "+rollValue.ToString()+" and amount: "+rollAmount.ToString()+".");
								if (rollAmount <= 0) {
									//Debug.Print ("Ending roll...");
									past = 255;
									present = 244;
									rolling = false;
									workMap[i][x, y] = file.ReadByte ();
									past = present;
									present = Convert.ToByte (workMap[i][x, y]);
									//Debug.Print ("Wrote: " + workMap [trueX, trueY].ToString () + " and present and past is: " + present.ToString () + " ; " + past.ToString () + ".");
									continue;
								}
								workMap[i][x, y] = rollValue;
								rollAmount--;
								continue;
							}

							workMap[i][x, y] = file.ReadByte ();
							past = present;
							present = Convert.ToByte (workMap[i][x, y]);
							//Debug.Print ("Wrote: " + workMap [trueX, trueY].ToString () + " and present and past is: " + present.ToString () + " ; " + past.ToString () + ".");
						} else {
							if (rollAmount <= 0) {
								//Debug.Print ("Ending roll...");
								past = 255;
								present = 244;
								rolling = false;
								workMap[i][x, y] = file.ReadByte ();
								past = present;
								present = Convert.ToByte (workMap[i][x, y]);
								//Debug.Print ("Wrote: " + workMap [trueX, trueY].ToString () + " and present and past is: " + present.ToString () + " ; " + past.ToString () + ".");
								continue;
							}
							workMap[i][x, y] = rollValue;
							rollAmount--;

						}
					}
				}
			}
			file.Close ();
			file.Dispose ();
			return Tuple.Create (mapWidth, mapHeight,layers, workMap);
		}







		/// <summary>
		/// Save method stolen- er... borrowed from An Excellent Map Editor.
		/// But I wrote it, so it's okay.
		/// 
		/// Forgets beginning data except for format version.
		/// </summary>
		public void save(){
			SaveFileDialog a = new SaveFileDialog();
			a.OverwritePrompt=false;
			a.Filter="Angry LegGuy files (*.AngryLegGuy)|*.AngryLegGuy|Angry Level files (*.AngryLevel)|*.AngryLevel|All files (*.*)|*.*";
			a.ShowDialog();
			FileStream happyfile = File.Open(a.FileName,FileMode.Create);
			BinaryWriter br = new BinaryWriter(happyfile);
			int numero=0;
			byte currentRun=255;
			bool doingRun = false;
			int runNumber = 0;
			int finishNumero = -80;
			byte past=254;
			byte present=255;
			// Map format versiom
			br.Write (Convert.ToByte(3));
			// THESE VALUES DONT MATTER FOR THE CUSTOM LOADING FUNCTION. WRITE DUMMY VALUES.
			// Width
			br.Write (Convert.ToByte(1));
			// Height
			br.Write (Convert.ToByte(1));
			// Layers?
			br.Write (Convert.ToByte(1));
				currentRun=255;
				doingRun = false;
				runNumber = 0;
				finishNumero = -80;
				for (int ya = 0; ya < 14; ya++) {
					for (int xa = 0; xa < 400; xa++) {
						topoffor:
						if (!doingRun) {
							past = present;
						present = Convert.ToByte ((songPlace.maparray [0] [xa, ya]));
							br.Write (present);	
							//Debug.Print ("Was: " + numero.ToString () + ".");
							numero += 1;
							//Debug.Print ("Wrote:" + byteworld [numero - 1].ToString());
							if (past==present) {
								//Debug.Print ("Good thing " + (numero - 2).ToString () + " isn't " + finishNumero.ToString () + ".");
								//Debug.Print ("Starting run with: " + byteworld [numero-1].ToString ());
								doingRun = true;
								currentRun = present;
							} else {
								//Debug.Print ("Numero:" + numero.ToString () + ".");
								//Debug.Print ("No pudedo. Last char is: " + byteworld [numero - 2].ToString () + " while this one is: " + byteworld [numero-1].ToString () + ".");
							}
						} else {
						if (songPlace.maparray [0] [xa, ya] == currentRun && runNumber<=254) {
								//Debug.Print ("Increment run number. "+world.maparray [xa, ya].ToString());
								runNumber += 1;
							} else {
								//Debug.Print("Going to finish and write:"+ Convert.ToByte ((runNumber)).ToString()+".");
								past=present;
								present = Convert.ToByte ((runNumber));
								br.Write (present);	
								finishNumero = numero;
								numero += 1;
								doingRun = false;
								currentRun = 0;
								runNumber = 0;
								past = 255;
								present = 254;
								//Debug.Print ("Finish numero is:" + finishNumero.ToString ());
								goto topoffor;
							}
						}

					}
				}
			if (doingRun) {
				Debug.Print ("a:" + runNumber.ToString ());
				past = present;
				present= Convert.ToByte ((runNumber));
				br.Write (present);	
				numero += 1;
				doingRun = false;
				currentRun = 0;
				runNumber = 0;
			}
			br.Close();

			if (showConfirmation) {
				MessageBox.Show ("Savedededed.");
			}

		}















	}
}

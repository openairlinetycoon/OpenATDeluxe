using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Path = System.IO.Path;
using Thread = System.Threading.Thread;

[Tool]
public class DockInterface : Control {
	public bool isDocked;

	private const string ATDPathConfig = "application/config/atd_path";
	LineEdit gamePath;
	static string path;
	Thread thread;

	CancellationTokenSource tokenS;

	Button importButton, cancelButton;
	Label currentlyLoading;
	Label brokenPath;
	static bool isLoading;

	public override void _ExitTree() {
		tokenS?.Cancel();
		GD.Print("DESTRUCTION!");
	}

	public override void _EnterTree() {

		try {
			GD.Print("Entered!");

			Node node = GetNode("Content/Scroll/TabContainer/Build Management/Reset");
			if (!node.IsConnected("pressed", this, nameof(ResetButtonPressed))) {
				node.Connect("pressed", this, nameof(ResetButtonPressed));
			}

			Node nodePrepare = GetNode("Content/Scroll/TabContainer/Build Management/PrepareBuild");
			if (!nodePrepare.IsConnected("pressed", this, nameof(PrepareBuild))) {
				nodePrepare.Connect("pressed", this, nameof(PrepareBuild));
			}

			importButton = (Button)GetNode("Content/Scroll/TabContainer/Images/HSplitContainer2/ExtractImages");
			if (!importButton.IsConnected("pressed", this, nameof(ImportImages))) {
				importButton.Connect("pressed", this, nameof(ImportImages));
			}
			cancelButton = (Button)GetNode("Content/Scroll/TabContainer/Images/HSplitContainer2/CancelImport");
			if (!cancelButton.IsConnected("pressed", this, nameof(CancelImport))) {
				cancelButton.Connect("pressed", this, nameof(CancelImport));
			}

			currentlyLoading = (Label)GetNode("Content/Scroll/TabContainer/Images/CurrentlyLoading");
			brokenPath = (Label)GetNode("Content/Scroll/TabContainer/Images/BrokenPath");

			gamePath = (LineEdit)GetNode("Content/Scroll/TabContainer/Images/GamePath");
			if (!gamePath.IsConnected("text_entered", this, nameof(EnterText))) {
				gamePath.Connect("text_entered", this, nameof(EnterText));
			}

			gamePath.Text = (string)SettingsManager.GetSetting(ATDPathConfig, "");
			EnterText(gamePath.Text);
		} catch (Exception e) {
			// GD.Print(e.Message);
			// GD.Print(e.StackTrace);
		}


		SetProcess(true);
	}

	private void CancelImport() {
		tokenS?.Cancel();
	}

	public override void _Process(float delta) {
		try {
			if (currentlyLoading == null && isDocked) {
				ResetButtonPressed();
			}

			importButton.Disabled = isLoading;
			currentlyLoading.Hide();
			cancelButton.Hide();
			if (isLoading && currentlyLoading != null) {
				currentlyLoading.Text = "Loading" + (((OS.GetTicksMsec() / 500) % 3 == 0) ? "." : ((OS.GetTicksMsec() / 500) % 3 == 1) ? ".." : "...");
				currentlyLoading.Show();
				cancelButton.Show();
			}
		} catch (Exception e) {
			// GD.Print(e.Message);
			// GD.Print(e.StackTrace);
		}
	}

	public void EnterText(string text) {
		GD.Print(text);
		if (!ATDGameLoader.IsOriginalGamePath(text)) {
			brokenPath.Show();
			importButton.Disabled = true;
		} else {
			importButton.Disabled = false;
			brokenPath.Hide();
			path = text;
			SettingsManager.SetSetting(ATDPathConfig, text);
		}
	}

	public void ImportImages() {
		if (importButton == null)
			return;

		tokenS = new CancellationTokenSource();
		importButton.Disabled = true;
		cancelButton.Show();
		currentlyLoading.Show();
		isLoading = true;
		ImportThread();
		//Godot.Thread thread = new Godot.Thread();
		//thread.Start(this, nameof(ImportThread));

	}

	private void ImportThread() {
		CancellationToken token = tokenS.Token;
		try {
			ATDataLoader.LoadImageData();
			ATDataLoader.CreateImageResources();
			ATDataLoader.SaveAllLoadedResources();
		} catch (AggregateException e) {
			GD.Print("FATAL ERROR!");
			GD.Print(e.Message);
			GD.Print(e.StackTrace);
		} catch (Exception e) {
			GD.Print("FATAL ERROR 2!");
			GD.Print(e.Message);
			GD.Print(e.StackTrace);
		}
		isLoading = false;
	}

	private void ResetButtonPressed() {
		ManageTool.Reload(this);
		GD.Print("RESET!");
	}


	private const string ImagesPath = "res://Images";

	public void PrepareBuild() {
		string bPath = ProjectSettings.GlobalizePath(ImagesPath);

		bool backup = ((CheckBox)GetNode("Content/Scroll/TabContainer/Build Management/Backup")).Pressed;
		if (backup) {
			Copy(bPath, bPath + "BACKUP");
			System.IO.File.WriteAllText(bPath + "BACKUP/.gdignore", string.Empty);
		}

		string[] files = Directory.GetFiles(bPath, "*", SearchOption.AllDirectories);

		//Hard coded empty ImageTexture .res file
		byte[] data = new byte[] { 82, 83, 67, 67, 2, 0, 0, 0, 0, 16, 0, 0, 20, 1, 0, 0, 154, 0, 0, 0, 40, 181, 47, 253, 96, 20, 0, 133, 4, 0, 162, 71, 26, 34, 160, 137, 4, 204, 0, 0, 0, 0, 68, 217, 1, 41, 121, 200, 78, 191, 147, 32, 249, 212, 79, 76, 182, 156, 101, 59, 33, 226, 232, 130, 139, 130, 72, 15, 12, 198, 193, 84, 86, 44, 203, 66, 49, 25, 118, 145, 213, 18, 225, 164, 172, 253, 5, 175, 199, 45, 197, 39, 65, 207, 46, 83, 133, 141, 185, 93, 133, 10, 249, 7, 103, 131, 14, 226, 116, 50, 45, 98, 14, 41, 38, 166, 40, 83, 123, 108, 179, 253, 137, 191, 112, 182, 231, 38, 249, 72, 133, 77, 227, 63, 0, 254, 192, 7, 14, 0, 106, 3, 64, 231, 186, 173, 114, 88, 19, 1, 23, 133, 21, 238, 50, 193, 130, 251, 151, 154, 226, 171, 75, 229, 39, 247, 22, 39, 215, 74, 214, 3, 192, 2, 82, 83, 67, 67 };

		GD.Print(bPath);
		GD.Print(files.Length);

		try {
			foreach (string file in files) {
				System.IO.File.WriteAllBytes(file, data);
			}
		} catch (Exception error) {
			GD.Print(error.Message);
			GD.Print(error.StackTrace);
		}

	}

	public static void Copy(string sourceDirectory, string targetDirectory) {
		var diSource = new DirectoryInfo(sourceDirectory);
		var diTarget = new DirectoryInfo(targetDirectory);

		CopyAll(diSource, diTarget);
	}

	public static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
		Directory.CreateDirectory(target.FullName);

		// Copy each file into the new directory.
		foreach (FileInfo fi in source.GetFiles()) {
			fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
		}


		// Copy each subdirectory using recursion.
		foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
			DirectoryInfo nextTargetSubDir =
				target.CreateSubdirectory(diSourceSubDir.Name);
			CopyAll(diSourceSubDir, nextTargetSubDir);
		}
	}

}
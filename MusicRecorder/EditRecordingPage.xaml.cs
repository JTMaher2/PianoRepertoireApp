﻿using Io.Github.Jtmaher2.MusicRecorder.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Io.Github.Jtmaher2.MusicRecorder
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditRecordingPage : ContentPage
    {
        private int mId;
        private string mOrigFileName;
        readonly IRecordAudio mAudioRecorderService;

        public EditRecordingPage(int id)
        {
            InitializeComponent();
            Populate(id);
            mAudioRecorderService = DependencyService.Resolve<IRecordAudio>();

        }

        private async void Populate(int id)
        {
            MusicRecording mr = await App.Database.GetItemAsync(id);
            mId = mr.ID;
            composerEnt.Text = mr.Composer;
            fileNameEnt.Text = mr.RecordingName;
            mOrigFileName = fileNameEnt.Text;
            notesEnt.Text = mr.Notes;
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await App.Database.SaveItemAsync(new MusicRecording
            {
                Composer = composerEnt.Text,
                RecordingName = fileNameEnt.Text,
                Notes = notesEnt.Text,
                ID = mId
            });

            if (Device.RuntimePlatform == Device.Android)
            {
                File.WriteAllBytes(FileSystem.AppDataDirectory + "/" + fileNameEnt.Text + ".opus", File.ReadAllBytes(FileSystem.AppDataDirectory + "/" + mOrigFileName + ".opus"));

                File.Delete(FileSystem.AppDataDirectory + "/" + mOrigFileName + ".opus");
            } else
            {
                // UWP
                mAudioRecorderService.WriteFile(fileNameEnt.Text, mOrigFileName);
            }
            await Navigation.PopModalAsync();
        }

        private void PreviewRecBtn_Clicked(object sender, EventArgs e)
        {
            if (previewRecBtn.Text == "Preview Recording")
            {
                mAudioRecorderService.PreviewRecording(fileNameEnt.Text + (Device.RuntimePlatform == Device.Android ? ".opus" : ".flac"), 0, 0);
                previewRecBtn.Text = "Stop";

                
            }
            else
            {
                mAudioRecorderService.StopPreviewRecording();
                previewRecBtn.Text = "Preview Recording";
            }

            // monitor when playback completes, so that button text can be changed
            void a()
            {
                while (!mAudioRecorderService.IsCompleted())
                {
                    System.Threading.Thread.Sleep(1000);
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    previewRecBtn.Text = "Preview Recording";
                });
            }

            new Task(a).Start();
        }

        protected override bool OnBackButtonPressed()
        {
            mAudioRecorderService.StopPreviewRecording();
            return base.OnBackButtonPressed();
        }
    }
}
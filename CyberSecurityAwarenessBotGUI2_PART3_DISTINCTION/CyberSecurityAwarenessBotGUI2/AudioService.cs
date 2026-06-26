using System;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace CyberSecurityAwarenessBotGUI2
{
    public class AudioService
    {
        private SoundPlayer greetingPlayer;

        public void PlayGreeting()
        {
            try
            {
                string audioPath = GetGreetingAudioPath();

                if (!File.Exists(audioPath))
                {
                    MessageBox.Show(
                        "Greeting audio was not found.\n\nExpected location:\n" + audioPath,
                        "Audio File Missing",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                greetingPlayer = new SoundPlayer(audioPath);
                greetingPlayer.Load();
                greetingPlayer.Play();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The greeting audio could not play.\n\nError:\n" + ex.Message,
                    "Audio Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public void StopGreeting()
        {
            try
            {
                if (greetingPlayer != null)
                {
                    greetingPlayer.Stop();
                }
            }
            catch
            {
                // Do nothing. The app must not crash because of audio.
            }
        }

        private string GetGreetingAudioPath()
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Assets",
                "greeting.wav"
            );
        }
    }
}
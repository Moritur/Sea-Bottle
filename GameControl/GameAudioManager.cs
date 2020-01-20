using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace GameControl
{
    /// <summary>
    /// Manages all audio in the game
    /// </summary>
    public static class GameAudioManager
    {

        /// <summary>
        /// Contains every <see cref="MediaPlayer"/> currently playing
        /// </summary>
        static readonly List<MediaPlayer> currentMediaPlayers = new List<MediaPlayer>();

        /// <summary>
        /// Contains each <see cref="MediaPlayer"/> which has finished and should be removed from <see cref="currentMediaPlayers"/>
        /// </summary>
        static readonly ConcurrentBag<MediaPlayer> finishedMediaPlayers = new ConcurrentBag<MediaPlayer>();


        static Uri musicBgUri = new Uri("Resources/music_bg.mp3", UriKind.Relative);
        static Uri bubblesUri = new Uri("Resources/bubbles.mp3", UriKind.Relative);
        static Uri shotUri = new Uri("Resources/shot_sound.mp3", UriKind.Relative);

        /// <summary>
        /// Is in-game audio muted?
        /// </summary>
        public static bool isAudioMuted { get; private set; } = false;

        /// <summary>
        /// Prepares audio for the game
        /// </summary>
        public static void Initialize()
        {
            MediaPlayer bgPlayer = new MediaPlayer();
            StartPlayer(musicBgUri, true);
        }

        /// <summary>
        /// Plays media from <see cref="Uri"/>
        /// </summary>
        /// <param name="uriToPlay">Path to file to be played</param>
        /// <param name="loop">Set to true to loop media</param>
        private static void StartPlayer(Uri uriToPlay, bool loop = false)
        {
            MediaPlayer player;

            #region remove old players
            while (finishedMediaPlayers.TryTake(out player))
            {
                currentMediaPlayers.Remove(player);
            }
            #endregion

            player = new MediaPlayer();
            player.Open(uriToPlay);

            currentMediaPlayers.Add(player);

            if (loop) player.MediaEnded += (object sender, EventArgs e) => ResetPlayer(player);
            else player.MediaEnded += (object sender, EventArgs e) => finishedMediaPlayers.Add(player);

            player.Play();
        }

        /// <summary>
        /// Mutes or unmutes audio in the game
        /// </summary>
        public static void SwitchMute()
        {
            isAudioMuted = !isAudioMuted;

            foreach (MediaPlayer mediaPlayer in currentMediaPlayers)
            {
                mediaPlayer.IsMuted = isAudioMuted;
            }
        }

        /// <summary>
        /// Plays shot sound
        /// </summary>
        public static void PlayShot()
        {
            StartPlayer(shotUri);
        }

        /// <summary>
        /// Plays bubbles sound
        /// </summary>
        public static void PlayBubbles()
        {
            StartPlayer(bubblesUri);
        }

        /// <summary>
        /// Resets <see cref="MediaPlayer"/> and plays it
        /// </summary>
        /// <remarks>
        /// Sets <see cref="MediaPlayer.Position"/> to <see cref="TimeSpan.Zero"/>
        /// and then calls <see cref="MediaPlayer.Play"/>
        /// </remarks>
        /// <param name="player"><see cref="MediaPlayer"/> to reset</param>
        private static void ResetPlayer(MediaPlayer player)
        {
            player.Position = TimeSpan.Zero;
            player.Play();
        }
    }
}

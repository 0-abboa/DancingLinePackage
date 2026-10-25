using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;

/// <summary>
/// 播放音乐
/// </summary>
namespace XcantloadX.DL.Music
{
    public class MusicPlayer : MonoBehaviour
    {
        
        private AudioSource audioSource;
        public MalodySong Song { get; private set;}

        [Header("Music")]


        [SerializeField]
        [NullCheck(true, "Please attach a AudioClip!")]
        private AudioClip audioClip; //待播放的 AudioClip
        public string beatmapPath;

        private double startTime = 0;
        [SerializeField] private double timePosition = 0; //当前的播放位置（秒）
        /// <summary>
        /// 当前播放位置（以节拍计）
        /// </summary>
        [SerializeField] private double beatPosition = 0; 
        /// <summary>
        /// 当前播放位置（以数组下标计）
        /// </summary>
        [SerializeField] private int currentIndex = 0;

        private bool inited = false; //是否已经初始化
        private bool initializing = false;
        private bool pendingPlay = false;

        private static MusicPlayer _main = null;
        public static MusicPlayer Main
        {
            get
            {
                if (MusicPlayer._main == null)
                    MusicPlayer._main = GameObject.FindObjectOfType<MusicPlayer>();
                return MusicPlayer._main;
            }
        }

        //节奏点事件
        public delegate void BeatHandler(double timePosition, double beatPosition, int beatIndex);
        public event BeatHandler BeatEvent;

        //节奏点丢失事件
        public delegate void BeatLossHandler(double lostBeatPosition, double currentBeatPosition);
        public event BeatLossHandler BeatLossEvent;

        public bool IsPlaying{ get; private set; }

        [Header("Offset")]
        /// <summary>
        /// 进行时间对比时允许的误差范围
        /// </summary>
        [Tooltip("The time offset when compare the current time with the beat time.")]
        public float AllowedTimeOffset = 0.05f;

        /// <summary>
        /// 自动调整 Time Offset
        /// </summary>
        public bool AutoAdjustOffset;

        private float evaluateStartTime = -1;
        private float evaluatePassedTime = 0;
        private int lossTimes = 0;
        private float lossPercent = 0;
        private float minDelayedTime = 0;

        /// <summary>
        /// 是否忽略节奏点丢失
        /// </summary>
        public bool IgnoreBeatLoss = true;

        

        void Start()
        {
            //TODO:设计Editor GUI
            
            StartCoroutine(Init());
        }


        void Update()
        {
            if (!this.IsPlaying || !this.inited)
                return;

            if (this.audioSource == null || this.audioSource.clip == null)
            {
                this.IsPlaying = false;
                return;
            }

            // Without the Malody chart, the line remains manually playable and
            // the assigned audio can finish, but beat-synchronized triggers stop.
            if (this.Song == null)
            {
                timePosition = this.audioSource.time;
                if (timePosition >= this.audioSource.clip.length)
                    this.IsPlaying = false;
                return;
            }

            //计算当前播放位置
            timePosition = audioSource.time + this.Song.Offset;
            beatPosition = (float)Math.Round(this.timePosition / this.Song.SecPerBeat, 1);
            if (timePosition > audioSource.clip.length) //如果播放完毕
            { 
                this.IsPlaying = false;
                return;
            }
            if (this.currentIndex >= this.Song.Beats.Length)
                return;


            bool haveBeatLoss = false; //是否检测到节奏点丢失
            float delayedTime = (float)Song.Beat2Time(Math.Abs(beatPosition - Song.Beats[this.currentIndex])); //计算理想时间与实际时间的差值

            //比较节拍位置
            if (delayedTime < this.AllowedTimeOffset)
            {
                if(this.BeatEvent != null)
                    this.BeatEvent(this.timePosition, this.beatPosition, this.currentIndex); //触发事件
                currentIndex++;
            }
            else if (beatPosition >= Song.Beats[currentIndex]) //如果当前位置比下一次节奏点位置要大，说明上一次没有检测到
            {
                haveBeatLoss = true;
                //进行自动检测调整
                if (this.AutoAdjustOffset)
                    AdjustTimeOffset(haveBeatLoss, delayedTime);

                if(this.IgnoreBeatLoss && this.BeatEvent != null)
                    this.BeatEvent(this.timePosition, this.beatPosition, this.currentIndex);
                else if(this.BeatLossEvent != null)
                    this.BeatLossEvent(Song.Beats[currentIndex], beatPosition); //触发事件

                //跳过所有的丢失节奏点
                int oldIndex = currentIndex;
                while (beatPosition >= Song.Beats[currentIndex])
                    currentIndex++;

                //输出警告
                Debug.LogWarning("Detected beat loss! Skiped " + (currentIndex - oldIndex) + " beats.");
            }

            //进行自动检测调整
            if (this.AutoAdjustOffset)
                AdjustTimeOffset(haveBeatLoss, delayedTime);
        }

        //初始化脚本
        private IEnumerator Init()
        {
            if (this.inited || this.initializing)
                yield break;

            this.initializing = true;

            //加载 AudioSource
            if (this.audioSource == null)
            {
                this.audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
                this.audioSource.clip = this.audioClip;
            }

            string path = ResolveBeatmapPath();
            string json = null;

#if UNITY_WEBGL && !UNITY_EDITOR
            // StreamingAssets is a URL in WebGL, so it must be read with UnityWebRequest.
            UnityWebRequest request = UnityWebRequest.Get(path);
            yield return request.Send();
            if (request.isError)
            {
                string error = request.error;
                request.Dispose();
                EnterAudioOnlyMode("Could not load beatmap at " + path + ": " + error);
                yield break;
            }
            json = request.downloadHandler.text;
            request.Dispose();
#else
            if (!File.Exists(path))
            {
                EnterAudioOnlyMode("Could not load beatmap. Tried: " + path);
                yield break;
            }
            json = File.ReadAllText(path);
#endif

            Exception parseException = null;
            try
            {
                LoadSong(MalodySong.CreateFromMalody(json));
            }
            catch (Exception exception)
            {
                parseException = exception;
            }

            if (parseException != null)
            {
                Debug.LogError("Could not parse beatmap at " + path + ": " + parseException.Message);
                this.initializing = false;
                this.pendingPlay = false;
                yield break;
            }

            this.inited = true;
            this.initializing = false;

            if (this.pendingPlay)
            {
                this.pendingPlay = false;
                Play();
            }
        }

        private string ResolveBeatmapPath()
        {
            if (!string.IsNullOrEmpty(this.beatmapPath) && File.Exists(this.beatmapPath))
                return this.beatmapPath;

            // Older scenes store an absolute Windows path. Reuse its filename and
            // resolve it from StreamingAssets so the project also works on the web.
            string configuredPath = (this.beatmapPath ?? string.Empty).Replace('\\', '/');
            int lastSlash = configuredPath.LastIndexOf('/');
            string fileName = lastSlash >= 0 ? configuredPath.Substring(lastSlash + 1) : configuredPath;
            if (string.IsNullOrEmpty(fileName))
                fileName = "1543646563.mc";

            return Application.streamingAssetsPath.TrimEnd('/') + "/" + fileName;
        }

        private void EnterAudioOnlyMode(string reason)
        {
            Debug.LogWarning(reason + " Continuing in audio-only mode; beat-triggered events are disabled.");
            this.inited = true;
            this.initializing = false;

            bool shouldPlay = this.pendingPlay;
            this.pendingPlay = false;
            if (shouldPlay)
                Play();
        }

        /// <summary>
        /// 自动调整 Time Offset
        /// </summary>
        /// <param name="beatLoss">是否有节奏点丢失</param>
        /// <param name="delayedTime">理想时间与实际时间的差值</param>
        private void AdjustTimeOffset(bool beatLoss, float delayedTime)
        {
            //初始化时间
            if (this.evaluateStartTime < 0)
                this.evaluateStartTime = Time.time;

            //更新时间、丢失次数、丢失率
            evaluatePassedTime = Time.time - evaluateStartTime;
            if (beatLoss)
                lossTimes++;
            lossPercent = lossTimes / evaluatePassedTime;

            //不在一个节奏点内且没有节奏点丢失
            if (delayedTime > AllowedTimeOffset && !beatLoss)
                return;

            //取得最小差值
            if (minDelayedTime == 0)
            {
                minDelayedTime = delayedTime;
                return;
            }
            if(delayedTime != 0)
                minDelayedTime = Math.Min(minDelayedTime, delayedTime);

            bool reset = false;

            if (lossPercent > 0.5) //如果丢拍率超过 50%，自动增加 Time Offset
            {
                AllowedTimeOffset = minDelayedTime * 1.1f;
                reset = true;
            }
            if (evaluatePassedTime > 2f)
            {
                AllowedTimeOffset = minDelayedTime * 1.02f; //将最小差值作为 Time Offset
                reset = true;
            }

            //重置数据
            if(reset)
            {
                this.evaluateStartTime = Time.time;
                this.lossTimes = 0;
                this.lossPercent = 0;
                this.minDelayedTime = 0;
            }
        }



        /// <summary>
        /// 加载指定歌曲
        /// </summary>
        /// <param name="song">Song.</param>
        public void LoadSong(MalodySong song)
        {
            this.Song = song;
        }

        /// <summary>
        /// 播放音频并开始触发 Trigger
        /// </summary>
        public void Play()
        {
            if (!this.inited)
            {
                this.pendingPlay = true;
                if (!this.initializing)
                    StartCoroutine(Init());
                return;
            }

            if (this.audioSource == null || this.audioSource.clip == null)
            {
                Debug.LogError("MusicPlayer is not ready. Check that an audio clip is assigned.");
                return;
            }

            startTime = AudioSettings.dspTime;
            this.audioSource.Play();
            this.IsPlaying = true;
        }

        /// <summary>
        /// 暂停播放
        /// </summary>
        public void Pause()
        {
            if (this.audioSource == null)
                return;

            this.audioSource.Pause();
            this.IsPlaying = false;
        }
    }

}

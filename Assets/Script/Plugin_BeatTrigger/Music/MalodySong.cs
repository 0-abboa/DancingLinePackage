using System.Collections;
using System.Collections.Generic;
using MojoJson;

/// <summary>
/// 歌曲
/// </summary>
public class MalodySong
{
	/// <summary>
	/// 歌曲的BPM
	/// </summary>
	public float Bpm { get; private set; }

	/// <summary>
	/// 歌曲的长度（单位：秒）
	/// </summary>
	public float LengthSec{ get; private set; }

	/// <summary>
	/// 每一拍的长度（单位：秒）
	/// </summary>
	public float SecPerBeat{ get; private set; }

    /// <summary>
    /// 歌曲的偏移（延迟多久播放）（单位：秒）
    /// </summary>
    public float Offset {get; private set;}

	/// <summary>
	/// 所有的节奏点
	/// </summary>
	/// <value>The beats.</value>
	public float[] Beats { get; private set; }

	/// <summary>
	/// 从 Malody 谱面创建 Song 对象
	/// </summary>
	/// <param name="json">Json 文本</param>
	public static MalodySong CreateFromMalody(string jsonString)
	{
		//解析 json
		MojoJson.JsonValue jsonObj = MojoJson.Json.Parse(jsonString);
		MalodySong song = new MalodySong();

		//获取所有节拍
		List<MojoJson.JsonValue> jsonBeats = jsonObj.AsObjectGetArray("note");
		List<float> beats = new List<float>();

		for (int i = 0; i < jsonBeats.Count; i++)
		{
			// "beat": [bar, numerator, denominator]
			MojoJson.JsonValue jsonBeat = jsonBeats[i];
			List<MojoJson.JsonValue> beatParts = jsonBeat.AsObjectGetArray("beat");
			int beatNum = beatParts[0].AsInt();
			int subBeatNum = beatParts[1].AsInt();
			int subBeatPerBeat = beatParts[2].AsInt();
			if (subBeatPerBeat <= 0)
				throw new System.FormatException("Beat denominator must be greater than zero.");

			if (beatNum == 0 && subBeatNum == 0 && subBeatPerBeat == 1)
			{
				// This marker starts the audio and is not a playable beat.
				if (!jsonBeat.AsObjectGetIsNull("offset"))
					song.Offset = jsonBeat.AsObjectGetFloat("offset") / 1000;
				continue;
			}

			beats.Add(beatNum + (subBeatNum / (float)subBeatPerBeat));
		}

		song.Beats = beats.ToArray();
		//获取 bpm
		song.Bpm = jsonObj.AsObjectGetArray("time")[0].AsObjectGetFloat("bpm"); //TODO:加入分段bpm的支持
		song.SecPerBeat = 60f / song.Bpm;
		return song;
	}

    /// <summary>
    /// 将秒转换为节拍
    /// </summary>
    /// <param name="time">秒</param>
    /// <returns>节拍</returns>
    public double Time2Beat(double time)
    {
        return Bpm * time / 60f;
    }

    /// <summary>
    /// 将节拍转换为秒
    /// </summary>
    /// <param name="beat">节拍</param>
    /// <returns>秒</returns>
    public double Beat2Time(double beat)
    {
        return beat / Bpm * 60f;
    }
}

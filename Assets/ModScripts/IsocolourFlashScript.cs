using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class IsocolourFlashScript : MonoBehaviour 
{
	public KMAudio Audio;
	public KMBombModule Module;

	public TextMesh Indicator;

	private static int _moduleIdCounter = 1;
	private int _moduleId;
	private bool _moduleSolved;

	private List<List<CFButton>> _sequences;
	private readonly List<CFButton> _submittedSequence = new List<CFButton>();

	private ICFGenerator _generator;
	private List<Slide> _slides;

	private void Awake()
    {

		_moduleId = _moduleIdCounter++;

    }

	
	private void Start()
	{
		_generator = new ICFGenerator((CFColor)Range(0, 6));

		_slides = _generator.GeneratedSlides;
		_sequences = _generator.GeneratedAnswers;
	}

	private IEnumerator DisplaySlides()
	{
		while (true)
		{
			foreach (var slide in _slides)
			{
				Indicator.text = slide.Word.ToString().ToUpperInvariant();
				Indicator.color = slide.GetColor();

				yield return new WaitForSeconds(0.75f);
			}

			Indicator.text = string.Empty;

			yield return new WaitForSeconds(1.5f);
		}
	}

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
    {
		var split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	private IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}
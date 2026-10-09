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

	public KMSelectable[] Buttons;

	public TextMesh Indicator;

	private static int _moduleIdCounter = 1;
	private int _moduleId;
	private bool _moduleSolved, _isActivated;

	private List<List<CFButton>> _sequences;
	private readonly List<CFButton> _submittedSequence = new List<CFButton>();

	private ICFGenerator _generator;
	private List<Slide> _slides;

	private readonly Coroutine[] _buttonAnims = new Coroutine[2];
	private Coroutine _slideCycle;

	private void Awake()
    {
		_moduleId = _moduleIdCounter++;
		
		foreach (var button in Buttons)
			button.OnInteract += () => { ButtonPress(button); return false; };
		
		Module.OnActivate += Activate;
    }

	
	private void Start()
	{
		Indicator.text = string.Empty;
		
		_generator = new ICFGenerator((CFColor)Range(0, 6));

		_slides = _generator.GeneratedSlides.ToList();
		_sequences = _generator.GeneratedAnswers.ToList();
	}

	private void Activate()
	{
		_isActivated = true;
		_slideCycle = StartCoroutine(DisplaySlides());
	}

	private void ButtonPress(KMSelectable button)
	{
		button.AddInteractionPunch(0.4f);

		var ix = (CFButton)Array.IndexOf(Buttons, button);
		
		if (_moduleSolved || !_isActivated)
			return;
		
		_submittedSequence.Add(ix);

		if (_submittedSequence.Count != 4)
			return;

		if (_sequences.First().SequenceEqual(_submittedSequence))
		{
			_sequences.RemoveAt(0);
			
			var slideIndex = ICFGenerator.GrabIndex(_submittedSequence);

			_slides[slideIndex % 8].Mark(slideIndex < 8);
			
			_submittedSequence.Clear();

			if (_sequences.Count != 0)
				return;
			
			_moduleSolved = true;
			Module.HandlePass();
		}
		else
		{
			Module.HandleStrike();

			_slides = _generator.GeneratedSlides.ToList();
			_sequences = _generator.GeneratedAnswers.ToList();
		}
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
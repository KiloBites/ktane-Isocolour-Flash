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

	private ICFGenerator _generator;
	private CFColor _selectedColor;

	private List<Slide> _slides;
	
	private readonly Coroutine[] _buttonAnims = new Coroutine[2];
	private Coroutine _slideCycle;

	private void Awake()
    {
		_moduleId = _moduleIdCounter++;
		
		Module.OnActivate += Activate;
    }
	
	private void Start()
	{
		Indicator.text = string.Empty;

		_selectedColor = (CFColor)Range(0, 6);
		
		_generator = new ICFGenerator(_selectedColor);

		_slides = _generator.GeneratedSlides;
	}

	private void Activate()
	{
		_isActivated = true;
		_slideCycle = StartCoroutine(CycleSlides());
	}

	private IEnumerator CycleSlides()
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

			yield return new WaitForSeconds(1);
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
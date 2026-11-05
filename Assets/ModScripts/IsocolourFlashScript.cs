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
	
	private CFMove _movePriority;

	private List<Slide> _slides;
	private List<List<Coord>> _answers;
	private List<CFMove> _applicableMoves;

	private bool[] _isMarked = new bool[16];
	
	private readonly List<CFButton> _submittedButtons = new List<CFButton>();
	
	private readonly Coroutine[] _buttonAnims = new Coroutine[2];

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

		_selectedColor = (CFColor)Range(0, 6);
		
		_generator = new ICFGenerator(_selectedColor);

		_slides = _generator.GeneratedSlides.ToList();
		_answers = _generator.GeneratedAnswers.ToList();
		_applicableMoves = _generator.ApplicableMoves;

		_movePriority = _applicableMoves.First();
		
		Log($"[Isocolour Flash #{_moduleId}] Presented slides:");
		
		foreach (var slide in _slides)
			Log($"[Isocolour Flash #{_moduleId}] {slide}");
		
		Log($"[Isocolour Flash #{_moduleId}] Initial grid: {_generator.LogGrid(true)}");

		foreach (var log in _generator.Logs)
			Log($"[Isocolour Flash #{_moduleId}] {log}");
		
		Log($"[Isocolour Flash #{_moduleId}] Final grid: {_generator.LogGrid(false)}");
		
		Log($"[Isocolour Flash #{_moduleId}] The coordinates to submit are: {_generator.ApplicableMoves.Select((x, i) => $"[{x}: {_answers[i].Join(", ")}]").Join(", ")}");
	}

	private void ButtonPress(KMSelectable button)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
		button.AddInteractionPunch(0.5f);

		var buttonIndex = (CFButton)Array.IndexOf(Buttons, button);

		if (_buttonAnims[(int)buttonIndex] != null)
		{
			StopCoroutine(_buttonAnims[(int)buttonIndex]);
			_buttonAnims[(int)buttonIndex] = null;
		}

		_buttonAnims[(int)buttonIndex] = StartCoroutine(PressAnimation(buttonIndex));

		if (_moduleSolved || !_isActivated)
			return;
		
		_submittedButtons.Add(buttonIndex);

		if (_submittedButtons.Count != 4)
			return;

		var inputToCoord = ICFGenerator.GetCoordFromInput(_submittedButtons);

		if (_generator.IsVoid(inputToCoord))
		{
			Log($"[Isocolour Flash #{_moduleId}] You are trying to submit a void square at {inputToCoord}. Strike!");
			Module.HandleStrike();
			
			_submittedButtons.Clear();
			
			return;
		}

		if (_isMarked[inputToCoord.Row * 4 + inputToCoord.Column])
		{
			Log($"[Isocolour Flash #{_moduleId}] You are inputting a coordinate that's already marked. Strike!");
			Module.HandleStrike();

			_submittedButtons.Clear();

			return;
		}
		

		if (_answers.First().Contains(inputToCoord))
		{
			Log($"[Isocolour Flash #{_moduleId}] {inputToCoord} has been marked successfully from the {_movePriority.ToString().ToLowerInvariant()} square group");
			
			_answers.First().Remove(inputToCoord);

			if (_answers.First().Count == 0)
			{
				_answers.RemoveAt(0);
				_applicableMoves.RemoveAt(0);
			}
			
			if (_answers.Count > 0)
				_movePriority = _applicableMoves.First();

			if (_answers.Count != 0)
			{
				Audio.PlaySoundAtTransform("Input Submit", transform);
				
				int slideIndex;
				var onWord = inputToCoord.CoordOnWord(out slideIndex);

				var modifiedSlide = _slides[slideIndex % 8];
				
				modifiedSlide.Mark(onWord);

				_slides[slideIndex % 8] = modifiedSlide;

				_isMarked[slideIndex] = true;

				_submittedButtons.Clear();
				
				return;
			}
			
			Log($"[Isocolour Flash #{_moduleId}] All non-void squares have been submitted. Solved!");
			
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
			
			_moduleSolved = true;
			
			Module.HandlePass();
		}
		else
		{
			var invalidIndex = _answers.IndexOf(x => x.Contains(inputToCoord));
			
			Log($"[Isocolour Flash #{_moduleId}] You tried to submit an coordinate ({inputToCoord}) that is only in the {_applicableMoves[invalidIndex].ToString().ToLowerInvariant()} square group. Strike!");
			
			Module.HandleStrike();

			_submittedButtons.Clear();

			_isMarked = new bool[16];

			_slides.Reinitialize(_generator.GeneratedSlides.ToList());
		}
	}

	private void Activate() => _isActivated = true;

	private void Update()
	{
		if (_moduleSolved || !_isActivated)
		{
			if (Indicator.text.Length != 0)
				Indicator.text = string.Empty;

			return;
		}

		var index = (int)(Time.time / 0.75f) % 9;
		
		ShowColor(index == 8 ? (Slide?)null : _slides[index]);
	}
	private IEnumerator PressAnimation(CFButton button)
	{
		const float duration = 0.1f;
		var elapsed = 0f;
		var curPos = Buttons[(int)button].transform.parent.localPosition;

		while (elapsed < duration)
		{
			Buttons[(int)button].transform.parent.localPosition = new Vector3(curPos.x, Easing.InOutQuad(elapsed, curPos.y, 0.01f, duration), curPos.z);
			yield return null;
			elapsed += Time.deltaTime;
		}

		Buttons[(int)button].transform.parent.localPosition = new Vector3(curPos.x, 0.01f, curPos.z);

		elapsed = 0;

		while (elapsed < duration)
		{
			Buttons[(int)button].transform.parent.localPosition = new Vector3(curPos.x, Easing.InOutQuad(elapsed, 0.01f, 0.0146f, duration), curPos.z);
			yield return null;
			elapsed += Time.deltaTime;
		}
		
		Buttons[(int)button].transform.parent.localPosition = new Vector3(curPos.x, 0.0146f, curPos.z);

		_buttonAnims[(int)button] = null;
	}

	private void ShowColor(Slide? slide)
	{
		Indicator.text = slide?.Word.ToString().ToUpperInvariant() ?? string.Empty;

		if (slide != null)
			Indicator.color = slide.Value.GetColor();
	}

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} press yynn/yes yes no no/1100 [presses the sequence of buttons]";
#pragma warning restore 414

	private IEnumerator ProcessTwitchCommand(string command)
    {
		var split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

		var validButtonCommands = new[] { "NO", "YES" };

		switch (split[0])
		{
			case "PRESS":
				if (split.Length == 1 || (split.Length > 2 && split.Skip(1).Count() != 4))
				{
					yield return "sendtochaterror Please specify what buttons to press!";
					yield break;
				}

				if (split[1].Length > 4 || (split.Length > 2 && split.Skip(1).Count() > 4))
				{
					yield return "sendtochaterror Too many parameters!";
					yield break;
				}

				List<CFButton> buttonsToPress;

				if (split.Length == 2)
				{
					if (split[1].All(char.IsDigit))
					{
						var digits = split[1].Select(x => x - '0').ToArray();

						if (digits.Any(x => x < 0 || x > 1))
						{
							yield return "sendtochaterror Digits must be 0-1 inclusive!";
							yield break;
						}
						
						buttonsToPress = digits.Select(x => (CFButton)x).ToList();
					}
					else
					{
						if (split[1].Any(x => !"NY".Contains(x)))
						{
							yield return "sendtochaterror You must either use N or Y when using this format!";
							yield break;
						}

						buttonsToPress = split[1].Select(x => (CFButton)"NY".IndexOf(x)).ToList();
					}
				}
				else
				{
					if (!split.Skip(1).All(validButtonCommands.Contains))
					{
						yield return $"sendtochaterror {split.Skip(1).Where(x => !validButtonCommands.Contains(x)).Join(", ")} isn't/aren't valid button command(s)!";
						yield break;
					}
					
					buttonsToPress = split.Skip(1).Select(x => (CFButton)Array.IndexOf(validButtonCommands, x)).ToList();
				}

				yield return null;

				foreach (var button in buttonsToPress)
				{
					Buttons[(int)button].OnInteract();
					yield return new WaitUntil(() => _buttonAnims[(int)button] == null);
				}
				
				yield break;
			default:
				yield return "sendtochaterror Invalid command!";
				yield break;
		}
		
		yield return null;
    }

	private IEnumerator TwitchHandleForcedSolve()
	{
		if (_submittedButtons.Any())
			_submittedButtons.Clear();

		while (_answers.Count > 0)
		{
			var buttonSeqSet = _answers.First().Select(ICFGenerator.CoordToSequence).ToList().Shuffle();
			
			foreach (var buttonSeq in buttonSeqSet)
				foreach (var button in buttonSeq)
				{
					Buttons[(int)button].OnInteract();
					yield return new WaitUntil(() => _buttonAnims[(int)button] == null);
				}
		}
	}


}
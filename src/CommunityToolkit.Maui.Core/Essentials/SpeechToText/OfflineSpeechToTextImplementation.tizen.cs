using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using Tizen.Uix.Stt;

namespace CommunityToolkit.Maui.Media;

/// <inheritdoc />
public sealed partial class OfflineSpeechToTextImplementation
{
	SttClient? sttClient;
	string defaultSttEngineLocale = "en_US";

	/// <inheritdoc/>
	public SpeechToTextState CurrentState => sttClient?.CurrentState is Tizen.Uix.Stt.State.Recording
		? SpeechToTextState.Listening
		: SpeechToTextState.Stopped;

	/// <inheritdoc />
	public ValueTask DisposeAsync()
	{
		if (sttClient is not null)
		{
			if (sttClient.CurrentState is Tizen.Uix.Stt.State.Ready)
			{
				sttClient.Unprepare();
			}
			else
			{
				sttClient.Cancel();
				sttClient.RecognitionResult -= OnRecognitionResult;
				sttClient.ErrorOccurred -= OnErrorOccurred;
				sttClient.StateChanged -= OnStateChanged;
			}

			sttClient.Dispose();

			sttClient = null;
		}

		return ValueTask.CompletedTask;
	}

	void InternalStopListening(in SttClient? sttClient)
	{
		if (sttClient is null)
		{
			return;
		}

		if (sttClient.CurrentState is Tizen.Uix.Stt.State.Recording)
		{
			sttClient.Stop();
		}

		sttClient.RecognitionResult -= OnRecognitionResult;
		sttClient.ErrorOccurred -= OnErrorOccurred;
		sttClient.StateChanged -= OnStateChanged;
	}

	void OnErrorOccurred(object? sender, ErrorOccurredEventArgs e)
	{
		InternalStopListening(sttClient);
		OnRecognitionResultCompleted(SpeechToTextResult.Failed(new Exception("STT failed - " + e.ErrorMessage)));
	}

	void OnRecognitionResult(object? sender, RecognitionResultEventArgs e)
	{
		if (e.Result is ResultEvent.Error)
		{
			InternalStopListening(sttClient);
			OnRecognitionResultCompleted(SpeechToTextResult.Failed(new Exception("Failure in speech engine - " + e.Message)));
		}
		else if (e.Result is ResultEvent.PartialResult)
		{
			foreach (var d in e.Data)
			{
				OnRecognitionResultUpdated(d);
			}
		}
		else
		{
			InternalStopListening(sttClient);
			OnRecognitionResultCompleted(SpeechToTextResult.Success(e.Data.ToString() ?? string.Empty));
		}
	}

	void OnStateChanged(object? sender, StateChangedEventArgs e)
	{
		OnSpeechToTextStateChanged(CurrentState);
	}

	[MemberNotNull(nameof(sttClient))]
	void Initialize(CancellationToken cancellationToken)
	{
		sttClient = new SttClient();
		
		try
		{
			sttClient.Prepare();
		}
		catch (Exception ex)
		{
			OnRecognitionResultCompleted(SpeechToTextResult.Failed(new Exception("STT is not available - " + ex)));
		}
	}

	void InternalStartListening(CultureInfo culture)
	{
		Initialize(cancellationToken);

		sttClient.ErrorOccurred += OnErrorOccurred;
		sttClient.RecognitionResult += OnRecognitionResult;
		sttClient.StateChanged += OnStateChanged;

		var recognitionType = sttClient.IsRecognitionTypeSupported(RecognitionType.Partial)
			? RecognitionType.Partial
			: RecognitionType.Free;

		sttClient.Start(defaultSttEngineLocale, recognitionType);
	}
}
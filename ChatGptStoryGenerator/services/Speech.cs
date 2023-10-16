using Microsoft.CognitiveServices.Speech;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGptStoryGenerator.services
{
    public static class Speech
    {
        public static async Task TextToSpeech(string text, string audioFilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    throw new ArgumentException("Text cannot be null or empty.");
                }

                if (string.IsNullOrEmpty(audioFilePath))
                {
                    throw new ArgumentException("Audio file path cannot be null or empty.");
                }

                var speechKey = "testkey";
                var regionKey = "eastus";

                var speechConfig = SpeechConfig.FromSubscription(speechKey, regionKey);

                speechConfig.SpeechSynthesisVoiceName = "en-US-AnaNeural";

                using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
                {
                    var speechResult = await speechSynthesizer.SpeakTextAsync(text);

                    if (speechResult.Reason == ResultReason.SynthesizingAudioCompleted)
                    {
                        using var stream = AudioDataStream.FromResult(speechResult);
                        await stream.SaveToWaveFileAsync(audioFilePath);
                        stream.Dispose();
                    }
                    else
                    {
                        Console.WriteLine($"Speech synthesis failed. Reason: {speechResult.Reason}");
                        throw new InvalidOperationException($"Speech synthesis failed. Reason: {speechResult.Reason}");
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"An argument exception occurred: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception("Speech synthesis failed.", ex);
            }
        }
    }
}
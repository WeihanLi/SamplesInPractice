using System.Diagnostics;
using System.Diagnostics.Tracing;
using WeihanLi.Extensions;

namespace HttpClientTest;

public class HttpClientEventSample
{
    public static async Task MainTest()
    {
        const string testApiUrl = "https://reservation.weihanli.xyz/health";
        DiagnosticListener.AllListeners.Subscribe(new CustomDiagnosticObserver());

        using var eventListener = new CustomEventListener();
        using var httpClient = new HttpClient();
        var result = await httpClient.GetStringAsync(testApiUrl);
        Console.WriteLine(result);
    }

    private sealed class CustomEventListener: EventListener
    {
        private static readonly HashSet<string> Sources = new()
        {
            "System.Net.Http",
            "System.Net.NameResolution",
            "System.Net.Sockets",
            
            // "Private.InternalDiagnostics.System.Net.Http",
            // "Private.InternalDiagnostics.System.Net.Sockets"
        };
        
        /// <summary>
        /// Override this method to get a list of all the eventSources that exist.  
        /// </summary>
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // Because we want to turn on every EventSource, we subscribe to a callback that triggers
            // when new EventSources are created.  It is also fired when the EventListener is created
            // for all pre-existing EventSources.  Thus this callback get called once for every 
            // EventSource regardless of the order of EventSource and EventListener creation.  

            // For any EventSource we learn about, turn it on.   
            if (Sources.Contains(eventSource.Name))
              EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
        }
        
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // report all event information
            Console.WriteLine("Event {0} ", $"{eventData.EventSource.Name} {eventData.EventName}");
            Console.WriteLine($"[{eventData.TimeStamp},{eventData.Level}]{eventData.Channel} {eventData.Opcode}");
            
            // Don't display activity information, as that's not used in the demos
            // Out.Write(" (activity {0}{1}) ", ShortGuid(eventData.ActivityId), 
            //                                  eventData.RelatedActivityId != Guid.Empty ? "->" + ShortGuid(eventData.RelatedActivityId) : "");

            // Events can have formatting strings 'the Message property on the 'Event' attribute.  
            // If the event has a formatted message, print that, otherwise print out argument values.  
            if (eventData.Message != null)
                Console.WriteLine(eventData.Message);
            if (eventData.Payload is { Count: > 0 })
            {
                Console.WriteLine(eventData.PayloadNames.ToJson());
                Console.WriteLine(eventData.Payload.ToJson());   
            }
            Console.WriteLine();
        }
    }
    
    private sealed class CustomDiagnosticObserver : IObserver<DiagnosticListener>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(DiagnosticListener value)
        {
            value.Subscribe(new KeyValueObserver());
        }
    }
    
    public class KeyValueObserver : IObserver<KeyValuePair<string, object>>
    {
        public void OnCompleted()
            => throw new NotImplementedException();

        public void OnError(Exception error)
            => throw new NotImplementedException();

        public void OnNext(KeyValuePair<string, object> value)
        {
            Console.WriteLine(value.ToJson());
        }
    }
}

using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Splat;
using ILogger = NuGet.Common.ILogger;
using NullLogger = NuGet.Common.NullLogger;

namespace RxUIStudyApp
{
    /// <summary>
    /// MVvm의 ObservableObject 와 같은 형태로 REactiveObject를 상속한 ViewModel이다.
    /// </summary>
    internal class AppViewModel : ReactiveObject
    {
        /// <summary>
        /// 일반 Property 이지만, 강제로 RaiseAndSetIfChanged 를통해서 Notification을 발생시킨다.
        /// </summary>
        private string _searchTerm ;
        public string SearchTerm
        {
            get => _searchTerm;
            set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
        }

        /// <summary>
        /// Reactive의 Observable 객체의 내부에서 subscribe 기능으로 새로운 값이 변경되면 Property notification이 발생되도록 하는 helper 객체이다.
        /// </summary>
        private readonly ObservableAsPropertyHelper<IEnumerable<NugetDetailsViewModel>> _searchResults;
        public IEnumerable<NugetDetailsViewModel> SearchResults => _searchResults.Value;

        // Here, we want to create a property to represent when the application 
        // is performing a search (i.e. when to show the "spinner" control that 
        // lets the user know that the app is busy). We also declare this property
        // to be the result of an Observable (i.e. its value is derived from 
        // some other property)
        private readonly ObservableAsPropertyHelper<bool> _isAvailable;
        public bool IsAvailable => _isAvailable.Value;

        public AppViewModel()
        {
            // Creating our UI declaratively
            // 
            // The Properties in this ViewModel are related to each other in different 
            // ways - with other frameworks, it is difficult to describe each relation
            // succinctly; the code to implement "The UI spinner spins while the search 
            // is live" usually ends up spread out over several event handlers.
            //
            // However, with ReactiveUI, we can describe how properties are related in a 
            // very organized clear way. Let's describe the workflow of what the user does 
            // in this application, in the order they do it.

            // We're going to take a Property and turn it into an Observable here - this
            // Observable will yield a value every time the Search term changes, which in
            // the XAML, is connected to the TextBox. 
            //
            // We're going to use the Throttle operator to ignore changes that happen too 
            // quickly, since we don't want to issue a search for each key pressed! We 
            // then pull the Value of the change, then filter out changes that are identical, 
            // as well as strings that are empty.
            //
            // We then do a SelectMany() which starts the task by converting Task<IEnumerable<T>> 
            // into IObservable<IEnumerable<T>>. If subsequent requests are made, the 
            // CancellationToken is called. We then ObserveOn the main thread, 
            // everything up until this point has been running on a separate thread due 
            // to the Throttle().
            //
            // We then use an ObservableAsPropertyHelper, OAPH, and the ToProperty() method to allow
            // us to have the latest results that we can expose through the property to the View.
            _searchResults = this
                .WhenAnyValue(x => x.SearchTerm)        // SearchTerm 텍스트가 입력된경우
                .Throttle(TimeSpan.FromMilliseconds(800))                   // Throttle 800ms 간격으로는 동작을 안함
                .Select(term => term?.Trim())                         // 결과 값을 trim 한다.
                .DistinctUntilChanged()                                     //
                .Where(term => !string.IsNullOrWhiteSpace(term))
                .SelectMany(SearchNuGetPackages)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.SearchResults);

            // We subscribe to the "ThrownExceptions" property of our OAPH, where ReactiveUI 
            // marshals any exceptions that are thrown in SearchNuGetPackages method. 
            // See the "Error Handling" section for more information about this.
            _searchResults.ThrownExceptions.Subscribe(error => { /* Handle errors here */ });

            // A helper method we can use for Visibility or Spinners to show if results are available.
            // We get the latest value of the SearchResults and make sure it's not null.
            _isAvailable = this
                .WhenAnyValue(x => x.SearchResults)
                .Select(searchResults => searchResults != null)
                .ToProperty(this, x => x.IsAvailable);

        }

        // Here we search NuGet packages using the NuGet.Client library. Ideally, we should
        // extract such code into a separate service, say, INuGetSearchService, but let's 
        // try to avoid overcomplicating things at this time.
        private async Task<IEnumerable<NugetDetailsViewModel>> SearchNuGetPackages(
            string term, CancellationToken token)
        {
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3()); // Add v3 API support
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            var source = new SourceRepository(packageSource, providers);
            ILogger logger = NullLogger.Instance;

            var filter = new SearchFilter(false);
            var resource = await source.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);
            var metadata = await resource.SearchAsync(term, filter, 0, 10, logger, token).ConfigureAwait(false);
            return metadata.Select(x => new NugetDetailsViewModel(x));
        }
    }
}

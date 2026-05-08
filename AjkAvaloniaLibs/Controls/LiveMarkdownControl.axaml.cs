using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AjkAvaloniaLibs.Controls;

public partial class LiveMarkdownControl : UserControl
{
    public LiveMarkdownControl()
    {
        InitializeComponent();

        // add math renderer
        LiveMarkdown.Avalonia.MarkdownNode.Register<LiveMarkdown.Avalonia.MathInlineNode>();
        LiveMarkdown.Avalonia.MarkdownNode.Register<LiveMarkdown.Avalonia.MathBlockNode>();

        // add image renderer
        //LiveMarkdown.Avalonia.AsyncImageLoader.DefaultDecoders =
        //    [   
        //        LiveMarkdown.Avalonia.SvgImageDecoder.Shared,
        //        LiveMarkdown.Avalonia.DefaultBitmapDecoder.Shared
        //    ];

        // add markdown renderer (need .net 10)
//        LiveMarkdown.Avalonia.MarkdownRenderer.ConfigurePipeline += x => x.UseMermaid();
//        LiveMarkdown.Avalonia.MarkdownNode.Register<LiveMarkdown.Avalonia.MermaidBlockNode>();

        Content = border;
        border.CornerRadius= new CornerRadius(5);
        border.BorderThickness = new Thickness(2);
        border.BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Gray);
//        border.Height = 200;

        border.Child = scrollViewer;
        scrollViewer.Content = markdownRenderer;
        scrollViewer.Margin= new Thickness(5);

        markdownRenderer.MarkdownBuilder = markdownStringBuilder;

        
    }
    /* add styles to App.xaml like this:
    <Application.Styles>
    <!-- Your other styles here -->
    <StyleInclude Source="avares://LiveMarkdown.Avalonia/Styles.axaml"/>
    </Application.Styles>

    <Application.Resources>
    <!-- Your other resources here -->
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="avares://LiveMarkdown.Avalonia/Defaults.axaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
    </Application.Resources> 
     */

    private Border border = new Border();
    private ScrollViewer scrollViewer = new ScrollViewer();
    private LiveMarkdown.Avalonia.ObservableStringBuilder markdownStringBuilder { get; } = new();
    private LiveMarkdown.Avalonia.MarkdownRenderer markdownRenderer = new LiveMarkdown.Avalonia.MarkdownRenderer();

    public string Text
    {
        set
        {
            markdownStringBuilder.Clear();
            markdownStringBuilder.Append(value);
        }
        get
        {
            return markdownStringBuilder.ToString();
        }
    }

    public string SelectedText
    {
        get
        {
            return markdownRenderer.SelectedText;
        }
    }
}

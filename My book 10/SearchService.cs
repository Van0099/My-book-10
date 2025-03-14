using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;

public class TextSearcher
{
    private readonly RichTextBox _richTextBox;
    private TextRange _lastSelection;

    public TextSearcher(RichTextBox richTextBox)
    {
        _richTextBox = richTextBox;
    }

    // Поиск следующего совпадения
    public bool FindNext(string searchText, bool matchCase = false)
    {
        if (string.IsNullOrEmpty(searchText))
            return false;

        // Начинаем поиск с текущего положения или с начала документа
        TextPointer start = _lastSelection != null ? _lastSelection.End : _richTextBox.Document.ContentStart;
        TextRange searchRange = new TextRange(start, _richTextBox.Document.ContentEnd);

        string documentText = matchCase ? searchRange.Text : searchRange.Text.ToLower();
        string searchQuery = matchCase ? searchText : searchText.ToLower();

        int index = documentText.IndexOf(searchQuery);
        if (index == -1)
            return false;

        TextPointer startPos = GetTextPositionAtOffset(_richTextBox.Document.ContentStart, index);
        TextPointer endPos = GetTextPositionAtOffset(startPos, searchText.Length);

        HighlightText(startPos, endPos);

        return true;
    }

    // Поиск предыдущего совпадения
    public bool FindPrevious(string searchText, bool matchCase = false)
    {
        if (string.IsNullOrEmpty(searchText))
            return false;

        // Начинаем поиск с текущего положения или с конца документа
        TextPointer end = _lastSelection != null ? _lastSelection.Start : _richTextBox.Document.ContentEnd;
        TextRange searchRange = new TextRange(_richTextBox.Document.ContentStart, end);

        string documentText = matchCase ? searchRange.Text : searchRange.Text.ToLower();
        string searchQuery = matchCase ? searchText : searchText.ToLower();

        int index = documentText.LastIndexOf(searchQuery);
        if (index == -1)
            return false;

        TextPointer startPos = GetTextPositionAtOffset(_richTextBox.Document.ContentStart, index);
        TextPointer endPos = GetTextPositionAtOffset(startPos, searchText.Length);

        HighlightText(startPos, endPos);

        return true;
    }

    // Подсветка найденного текста
    private void HighlightText(TextPointer start, TextPointer end)
    {
        _lastSelection = new TextRange(start, end);
        _lastSelection.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
    }

    public void ClearHighlight()
    {
        TextRange allText = new TextRange(_richTextBox.Document.ContentStart, _richTextBox.Document.ContentEnd);
        allText.ClearAllProperties();
    }

    // Получаем позицию текста на основе смещения
    private TextPointer GetTextPositionAtOffset(TextPointer start, int offset)
    {
        TextPointer current = start;
        while (offset > 0 && current != null)
        {
            if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                int count = current.GetTextRunLength(LogicalDirection.Forward);
                if (offset <= count)
                    return current.GetPositionAtOffset(offset);

                offset -= count;
            }
            current = current.GetNextContextPosition(LogicalDirection.Forward);
        }
        return current;
    }
}

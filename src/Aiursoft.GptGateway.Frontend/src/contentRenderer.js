import hljs from "highlight.js";
import "highlight.js/styles/github.css";
import MarkdownIt from "markdown-it";
const contentRenderer = new MarkdownIt({
  highlight: function (str, lang) {
    if (lang && hljs.getLanguage(lang)) {
      try {
        return (
          '<pre class="hljs"><span><code>' +
          hljs.highlight(str, { language: lang, ignoreIllegals: true }).value +
          "</code></span></pre>"
        );
      } catch (__) {}
    }

    return '<pre class="hljs"><code>' + contentRenderer.utils.escapeHtml(str) + "</code></pre>";
  },
});

export { contentRenderer };

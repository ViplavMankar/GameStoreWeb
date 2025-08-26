window.initTinyMCE = (selector, content) => {
    console.log("Initializing TinyMCE for", selector);
    if (tinymce.get(selector)) {
        tinymce.remove(`#${selector}`);
    }
    tinymce.init({
        selector: `#${selector}`,
        height: 500,
        menubar: true,
        plugins: [
            'advlist autolink lists link image charmap print preview anchor',
            'searchreplace visualblocks code fullscreen',
            'insertdatetime media table paste code help wordcount'
        ],
        toolbar: 'undo redo | formatselect | ' +
            'bold italic backcolor | alignleft aligncenter ' +
            'alignright alignjustify | bullist numlist outdent indent | ' +
            'removeformat | help',
        setup: (editor) => {
            editor.on('init', () => {
                if (content) {
                    editor.setContent(content); // ✅ Set initial content here
                }
            });
            editor.on('change', () => {
                editor.save();
            });
        }
    });
};

window.getTinyMCEContent = (selector) => {
    const editor = tinymce.get(selector);
    if (editor) {
        editor.save(); // Ensure the content is saved before retrieving
        return editor.getContent();
    }
    return '';
};

window.setTinyMCEContent = (id, html) => {
    const ed = tinymce.get(id);
    if (!ed) return;
    ed.setContent(html || "");
};
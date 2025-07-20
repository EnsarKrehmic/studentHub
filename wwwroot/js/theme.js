document.addEventListener('DOMContentLoaded', function () {
    const darkModeSwitch = document.getElementById('darkModeSwitch');
    const htmlElement = document.documentElement;

    // Load saved theme or default to light
    const savedTheme = localStorage.getItem('theme') || 'light';
    htmlElement.setAttribute('data-bs-theme', savedTheme);
    darkModeSwitch.checked = savedTheme === 'dark';

    // Toggle theme
    darkModeSwitch.addEventListener('change', function () {
        const theme = this.checked ? 'dark' : 'light';
        htmlElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('theme', theme);
    });
});

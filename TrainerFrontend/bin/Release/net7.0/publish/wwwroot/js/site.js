window.setDarkMode = (isDarkMode) => {
    document.body.classList.toggle('dark-mode', isDarkMode);
    localStorage.setItem('darkMode', isDarkMode);
};

window.getDarkMode = () => {
    return localStorage.getItem('darkMode') === 'true';
};
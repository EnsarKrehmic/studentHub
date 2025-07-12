document.addEventListener('DOMContentLoaded', function () {
    // === Panel open/close logic ===
    var fab = document.getElementById('accessibilityFab');
    var panel = document.getElementById('accessibilityPanel');
    var closeBtn = document.getElementById('accessibilityPanelClose');

    // Automatski otvori panel nakon reload-a (npr. zbog promjene jezika)
    if (localStorage.getItem('openAccessibilityPanel') === '1') {
        if (panel) {
            panel.classList.remove('d-none');
            localStorage.removeItem('openAccessibilityPanel');
        }
    }
    if (fab && panel && closeBtn) {
        fab.addEventListener('click', function () {
            panel.classList.remove('d-none');
        });
        closeBtn.addEventListener('click', function () {
            panel.classList.add('d-none');
        });
    }
    var hideBtn = document.getElementById('hideAccessibilityPanel');
    if (hideBtn) {
        hideBtn.addEventListener('click', function () {
            if (panel) panel.classList.add('d-none');
        });
    }

    // === Jezik switcher ===
    var langSelect = document.getElementById('languageSelect');
    if (langSelect) {
        langSelect.addEventListener('change', function () {
            var selectedLang = langSelect.value;
            var returnUrl = window.location.pathname + window.location.search;
            localStorage.setItem('openAccessibilityPanel', '1');
            window.location.href = '/Home/SetLanguage?culture=' + selectedLang + '&returnUrl=' + encodeURIComponent(returnUrl);
        });
    }

    // === Reset settings ===
    var resetBtn = document.getElementById('resetAccessibilitySettings');
    if (resetBtn) {
        resetBtn.addEventListener('click', function () {
            // Ukloni sve klase i localStorage preference vezane za pristupačnost
            var classes = [
                'access-high-contrast', 'access-vision-impaired', 'access-adhd-friendly', 'access-dyslexia-font',
                'access-keyboard-navigation', 'access-blind-users', 'access-readable-font',
                'access-highlight-titles', 'access-highlight-links', 'access-align-left',
                'access-align-center', 'access-align-right', 'access-dark-contrast', 'access-light-contrast',
                'access-monochrome', 'access-high-saturation', 'access-low-saturation', 'access-hide-images',
                'access-mute-sounds', 'access-stop-animations', 'access-highlight-focus'
            ];
            classes.forEach(cls => {
                document.body.classList.remove(cls);
                document.documentElement.classList.remove(cls);
            });
            document.body.removeAttribute('data-content-scale');
            document.body.removeAttribute('data-font-size');
            document.body.removeAttribute('data-line-height');
            document.body.removeAttribute('data-letter-spacing');
            document.body.removeAttribute('data-text-color');
            document.body.removeAttribute('data-title-color');
            document.body.removeAttribute('data-background-color');
            localStorage.clear();
            // Resetuj sve checkboxove i vrijednosti
            document.querySelectorAll('.form-check-input[type="checkbox"]').forEach(function (cb) { cb.checked = false; });
            document.getElementById('contentScaleValue').innerText = "100%";
            document.getElementById('fontSizeValue').innerText = "16px";
            document.getElementById('lineHeightValue').innerText = "1.5";
            document.getElementById('letterSpacingValue').innerText = "normal";
        });
    }

    // Bootstrap 4 modal trigger za 'Statement'
    var statementBtn = document.getElementById('accessibilityStatement');
    if (statementBtn) {
        statementBtn.addEventListener('click', function () {
            $('#accessibilityStatementModal').modal('show');
        });
    }

    // Prikaz/skrivanje Useful Links boxa
    var usefulLinksBtn = document.getElementById('usefulLinks');
    var usefulLinksPanel = document.getElementById('usefulLinksPanel');
    if (usefulLinksBtn && usefulLinksPanel) {
        usefulLinksBtn.addEventListener('click', function () {
            usefulLinksPanel.style.display = (usefulLinksPanel.style.display === 'none' || !usefulLinksPanel.style.display) ? 'block' : 'none';
        });
    }
    // Klik na opciju otvara link (novi tab ili email)
    document.querySelectorAll('.useful-link-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var url = btn.getAttribute('data-url');
            if (url.startsWith('mailto:')) {
                window.location.href = url;
            } else {
                window.open(url, '_blank');
            }
        });
    });

    // === Profili ===
    function setToggleProfile(id, cls, storageKey) {
        var cb = document.getElementById(id);
        if (!cb) return;
        cb.checked = !!localStorage.getItem(storageKey);
        setProfileClass(cb.checked, cls, storageKey);

        cb.addEventListener('change', function () {
            setProfileClass(cb.checked, cls, storageKey);
        });
    }
    function setProfileClass(enabled, cls, storageKey) {
        if (enabled) {
            document.body.classList.add(cls);
            document.documentElement.classList.add(cls);
            localStorage.setItem(storageKey, '1');
        } else {
            document.body.classList.remove(cls);
            document.documentElement.classList.remove(cls);
            localStorage.removeItem(storageKey);
        }
    }
    setToggleProfile('highContrastProfile', 'access-high-contrast', 'accessHighContrast');
    setToggleProfile('visionImpairedProfile', 'access-vision-impaired', 'accessVisionImpaired');
    setToggleProfile('adhdFriendlyProfile', 'access-adhd-friendly', 'accessAdhdFriendly');
    setToggleProfile('dyslexiaProfile', 'access-dyslexia-font', 'accessDyslexiaFont');
    setToggleProfile('keyboardNavigationProfile', 'access-keyboard-navigation', 'accessKeyboardNav');
    setToggleProfile('blindUsersProfile', 'access-blind-users', 'accessBlindUsers');
    setToggleProfile('seizureSafeProfile', 'access-seizure-safe', 'accessSeizureSafe');

    // === Ostale opcije ===
    setToggleProfile('readableFont', 'access-readable-font', 'accessReadableFont');
    setToggleProfile('highlightTitles', 'access-highlight-titles', 'accessHighlightTitles');
    setToggleProfile('highlightLinks', 'access-highlight-links', 'accessHighlightLinks');

    // === Cognitive Disability Profile (pr: automatski uključuje readable font i highlight titles) ===
    var cogCB = document.getElementById('cognitiveDisabilityProfile');
    if (cogCB) {
        cogCB.checked = !!localStorage.getItem('accessCognitiveDisability');
        if (cogCB.checked) {
            document.body.classList.add('access-readable-font');
            document.body.classList.add('access-highlight-titles');
        }
        cogCB.addEventListener('change', function () {
            if (cogCB.checked) {
                document.body.classList.add('access-readable-font');
                document.body.classList.add('access-highlight-titles');
                localStorage.setItem('accessCognitiveDisability', '1');
            } else {
                document.body.classList.remove('access-readable-font');
                document.body.classList.remove('access-highlight-titles');
                localStorage.removeItem('accessCognitiveDisability');
            }
        });
    }

    // === Content scaling ===
    var contentScale = parseInt(localStorage.getItem('accessContentScale') || "100", 10);
    var scaleValueSpan = document.getElementById('contentScaleValue');
    function applyContentScale() {
        document.body.setAttribute('data-content-scale', '1');
        document.body.style.setProperty('--access-content-scale', contentScale + '%');
        if (scaleValueSpan) scaleValueSpan.innerText = contentScale + "%";
        localStorage.setItem('accessContentScale', contentScale);
    }
    if (scaleValueSpan) scaleValueSpan.innerText = contentScale + "%";
    var btnScaleUp = document.getElementById('contentScaleUp');
    var btnScaleDown = document.getElementById('contentScaleDown');
    if (btnScaleUp) btnScaleUp.addEventListener('click', function () {
        if (contentScale < 200) { contentScale += 10; applyContentScale(); }
    });
    if (btnScaleDown) btnScaleDown.addEventListener('click', function () {
        if (contentScale > 50) { contentScale -= 10; applyContentScale(); }
    });
    applyContentScale();

    // === Font size ===
    var fontSize = parseInt(localStorage.getItem('accessFontSize') || "16", 10);
    var fontSizeValue = document.getElementById('fontSizeValue');
    function applyFontSize() {
        document.body.setAttribute('data-font-size', '1');
        document.body.style.setProperty('--access-font-size', fontSize + "px");
        if (fontSizeValue) fontSizeValue.innerText = fontSize + "px";
        localStorage.setItem('accessFontSize', fontSize);
    }
    if (fontSizeValue) fontSizeValue.innerText = fontSize + "px";
    var btnFontUp = document.getElementById('fontSizeUp');
    var btnFontDown = document.getElementById('fontSizeDown');
    if (btnFontUp) btnFontUp.addEventListener('click', function () {
        if (fontSize < 32) { fontSize += 1; applyFontSize(); }
    });
    if (btnFontDown) btnFontDown.addEventListener('click', function () {
        if (fontSize > 12) { fontSize -= 1; applyFontSize(); }
    });
    applyFontSize();

    // === Line height ===
    var lineHeight = parseFloat(localStorage.getItem('accessLineHeight') || "1.5");
    var lineHeightValue = document.getElementById('lineHeightValue');
    function applyLineHeight() {
        document.body.setAttribute('data-line-height', '1');
        document.body.style.setProperty('--access-line-height', lineHeight);
        if (lineHeightValue) lineHeightValue.innerText = lineHeight;
        localStorage.setItem('accessLineHeight', lineHeight);
    }
    if (lineHeightValue) lineHeightValue.innerText = lineHeight;
    var btnLHUp = document.getElementById('lineHeightUp');
    var btnLHDown = document.getElementById('lineHeightDown');
    if (btnLHUp) btnLHUp.addEventListener('click', function () {
        if (lineHeight < 3) { lineHeight += 0.1; lineHeight = Math.round(lineHeight * 10) / 10; applyLineHeight(); }
    });
    if (btnLHDown) btnLHDown.addEventListener('click', function () {
        if (lineHeight > 1) { lineHeight -= 0.1; lineHeight = Math.round(lineHeight * 10) / 10; applyLineHeight(); }
    });
    applyLineHeight();

    // === Letter spacing ===
    var letterSpacing = parseFloat(localStorage.getItem('accessLetterSpacing') || "0");
    var letterSpacingValue = document.getElementById('letterSpacingValue');
    function applyLetterSpacing() {
        document.body.setAttribute('data-letter-spacing', '1');
        document.body.style.setProperty('--access-letter-spacing', letterSpacing + "em");
        if (letterSpacingValue) letterSpacingValue.innerText = (letterSpacing === 0 ? "normal" : letterSpacing.toFixed(2));
        localStorage.setItem('accessLetterSpacing', letterSpacing);
    }
    if (letterSpacingValue) letterSpacingValue.innerText = (letterSpacing === 0 ? "normal" : letterSpacing.toFixed(2));
    var btnLSUp = document.getElementById('letterSpacingUp');
    var btnLSDown = document.getElementById('letterSpacingDown');
    if (btnLSUp) btnLSUp.addEventListener('click', function () {
        if (letterSpacing < 0.3) { letterSpacing += 0.01; letterSpacing = Math.round(letterSpacing * 100) / 100; applyLetterSpacing(); }
    });
    if (btnLSDown) btnLSDown.addEventListener('click', function () {
        if (letterSpacing > 0) { letterSpacing -= 0.01; letterSpacing = Math.max(0, Math.round(letterSpacing * 100) / 100); applyLetterSpacing(); }
    });
    applyLetterSpacing();

    // === Align buttons ===
    var alignBtns = {
        alignLeft: 'access-align-left',
        alignCenter: 'access-align-center',
        alignRight: 'access-align-right'
    };
    Object.keys(alignBtns).forEach(function (btnId) {
        var btn = document.getElementById(btnId);
        if (btn) {
            btn.addEventListener('click', function () {
                Object.values(alignBtns).forEach(function (cls) {
                    document.body.classList.remove(cls);
                });
                document.body.classList.add(alignBtns[btnId]);
                localStorage.setItem('accessAlign', alignBtns[btnId]);
            });
        }
    });
    // Apply saved alignment
    var align = localStorage.getItem('accessAlign');
    if (align) document.body.classList.add(align);

    // === Color adjustments: Kontrast, Saturacija, Monochrome ===
    function toggleSingleClass(className, storageKey) {
        var current = document.body.classList.contains(className);
        if (current) {
            document.body.classList.remove(className);
            document.documentElement.classList.remove(className);
            localStorage.removeItem(storageKey);
        } else {
            document.body.classList.add(className);
            document.documentElement.classList.add(className);
            localStorage.setItem(storageKey, '1');
        }
    }
    function bindSingleBtn(btnId, className, storageKey) {
        var btn = document.getElementById(btnId);
        if (btn) btn.addEventListener('click', function () {
            toggleSingleClass(className, storageKey);
        });
        // On load
        if (localStorage.getItem(storageKey)) {
            document.body.classList.add(className);
            document.documentElement.classList.add(className);
        }
    }
    bindSingleBtn('darkContrast', 'access-dark-contrast', 'accessDarkContrast');
    bindSingleBtn('lightContrast', 'access-light-contrast', 'accessLightContrast');
    bindSingleBtn('monochrome', 'access-monochrome', 'accessMonochrome');
    bindSingleBtn('highSaturation', 'access-high-saturation', 'accessHighSaturation');
    bindSingleBtn('lowSaturation', 'access-low-saturation', 'accessLowSaturation');
    bindSingleBtn('highContrastBtn', 'access-high-contrast', 'accessHighContrastBtn'); // button, not profile

    // === Color Pickers ===
    function setColor(type, color) {
        if (type === "text") {
            document.body.setAttribute('data-text-color', '1');
            document.body.style.setProperty('--access-text-color', color);
            localStorage.setItem('accessTextColor', color);
        } else if (type === "title") {
            document.body.setAttribute('data-title-color', '1');
            document.body.style.setProperty('--access-title-color', color);
            localStorage.setItem('accessTitleColor', color);
        } else if (type === "background") {
            document.body.setAttribute('data-background-color', '1');
            document.body.style.setProperty('--access-background-color', color);
            localStorage.setItem('accessBackgroundColor', color);
        }
    }
    document.querySelectorAll('.color-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var type = btn.getAttribute('data-type');
            var color = btn.getAttribute('data-color');
            setColor(type, color);
        });
    });
    // Apply saved custom colors
    var txtColor = localStorage.getItem('accessTextColor');
    if (txtColor) setColor('text', txtColor);
    var titleColor = localStorage.getItem('accessTitleColor');
    if (titleColor) setColor('title', titleColor);
    var bgColor = localStorage.getItem('accessBackgroundColor');
    if (bgColor) setColor('background', bgColor);

    // Cancel color pickers
    var cancelTextColor = document.getElementById('cancelTextColor');
    if (cancelTextColor) cancelTextColor.addEventListener('click', function () {
        document.body.removeAttribute('data-text-color');
        document.body.style.removeProperty('--access-text-color');
        localStorage.removeItem('accessTextColor');
    });
    var cancelTitleColor = document.getElementById('cancelTitleColor');
    if (cancelTitleColor) cancelTitleColor.addEventListener('click', function () {
        document.body.removeAttribute('data-title-color');
        document.body.style.removeProperty('--access-title-color');
        localStorage.removeItem('accessTitleColor');
    });
    var cancelBackgroundColor = document.getElementById('cancelBackgroundColor');
    if (cancelBackgroundColor) cancelBackgroundColor.addEventListener('click', function () {
        document.body.removeAttribute('data-background-color');
        document.body.style.removeProperty('--access-background-color');
        localStorage.removeItem('accessBackgroundColor');
    });

    // === Orijentacija, mute, hide images, stop animations, highlight focus ===
    bindSingleBtn('muteSounds', 'access-mute-sounds', 'accessMuteSounds');
    bindSingleBtn('hideImages', 'access-hide-images', 'accessHideImages');
    bindSingleBtn('stopAnimations', 'access-stop-animations', 'accessStopAnimations');
    bindSingleBtn('highlightFocus', 'access-highlight-focus', 'accessHighlightFocus');

    // === Useful Links (prikaz demo panela) ===
    var usefulLinksBtn = document.getElementById('usefulLinks');
    var usefulLinksPanel = document.getElementById('usefulLinksPanel');
    if (usefulLinksBtn && usefulLinksPanel) {
        usefulLinksBtn.addEventListener('click', function () {
            usefulLinksPanel.classList.toggle('active');
        });
    }
});

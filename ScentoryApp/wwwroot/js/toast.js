(function(){
    function ensureContainer(){
        const id = 'site-toast-container';
        let c = document.getElementById(id);
        if(!c){
            c = document.createElement('div');
            c.id = id;
            c.className = 'site-toast-container';
            document.body.appendChild(c);
        }
        return c;
    }

    window.showToast = function(message, type = 'info', timeout = 3500){
        try{
            const container = ensureContainer();
            const toast = document.createElement('div');
            toast.className = 'site-toast site-toast-' + (type || 'info');

            const msg = document.createElement('div');
            msg.className = 'site-toast-message';
            // Convert newlines to <br> tags for proper line breaks
            msg.innerHTML = (message || '').replace(/\n/g, '<br>');
            toast.appendChild(msg);

            const close = document.createElement('button');
            close.className = 'site-toast-close';
            close.type = 'button';
            close.setAttribute('aria-label','Close');
            close.innerHTML = '\u00d7';
            toast.appendChild(close);

            close.addEventListener('click', function(){
                hide(toast);
            });

            container.appendChild(toast);

            // show (allow CSS transition)
            window.getComputedStyle(toast).opacity;
            toast.classList.add('site-toast-visible');

            const t = setTimeout(function(){ hide(toast); }, timeout);

            function hide(el){
                clearTimeout(t);
                el.classList.remove('site-toast-visible');
                setTimeout(function(){
                    if(el && el.parentNode) el.parentNode.removeChild(el);
                }, 300);
            }
        }catch(e){
            console.error('showToast error', e);
        }
    };
})();

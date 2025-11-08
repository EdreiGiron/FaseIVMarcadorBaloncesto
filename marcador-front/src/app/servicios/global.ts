export const Global = {
    apiBase: 'http://basketmarcador.online/api',

    endpoints: {
        jwks: 'http://basketmarcador.online/.well-known/jwks.json',
        auth: 'http://basketmarcador.online/api/auth',
        oauth:'http://basketmarcador.online/auth',          
        me: 'http://basketmarcador.online/api/auth',
        equipos: 'http://basketmarcador.online/api/equipos',
        jugadores: 'http://basketmarcador.online/api/jugadores',
        partidos: 'http://basketmarcador.online/api/partidos',
        torneos: 'http://basketmarcador.online/api/torneos',
        reportes: 'http://basketmarcador.online/api/reportes',
        marcador: 'http://basketmarcador.online/api/marcador'
    },

    reportesUrl: 'http://basketmarcador.online/api/reportes',

    FRONT_ORIGIN: window.location.origin,
    OAUTH_CALLBACK_PATH: '/auth/callback',
    STORAGE: { ACCESS: 'auth.access', REFRESH: 'auth.refresh', USER: 'auth.user', ROLE: 'auth.role' },
};

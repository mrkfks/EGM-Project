const _host = typeof window !== 'undefined' ? window.location.hostname : 'localhost';

export const environment = {
  production: false,
  apiUrl: `http://${_host}:5117`,
  apiBaseUrl: `http://${_host}:5117`
};

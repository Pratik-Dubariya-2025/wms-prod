import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './app/App';
import { applyTheme, getInitialTheme } from './store/uiStore';

// Apply the persisted theme (default light) before first paint to avoid a flash.
applyTheme(getInitialTheme());

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);

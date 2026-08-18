import { AppProviders } from './AppProviders';
import { AppRouter } from './AppRouter';
import { AuthInitializer } from '@/components/shared/AuthInitializer';
import { useSignalR } from '@/hooks/useSignalR';

function App() {
  // Connect/disconnect SignalR based on authentication state
  useSignalR();

  return (
    <AppProviders>
      <AuthInitializer>
        <AppRouter />
      </AuthInitializer>
    </AppProviders>
  );
}

export default App;

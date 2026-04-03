/**
 * leaflet.heat ve @microsoft/signalr için minimal tip bildirimleri.
 * Bu paketler kurulduktan sonra gerçek tipler otomatik devreye girer.
 */

declare module 'leaflet.markercluster';

declare module 'leaflet.heat' {
  // leaflet.heat bir Leaflet plugin'idir — L.heatLayer() global metodunu ekler.
  // Kendi başına dışa aktarma yapmaz; yan-etki import'u olarak kullanılır.
  const _default: any;
  export default _default;
}

declare module '@microsoft/signalr' {
  export const LogLevel: {
    Trace: 0; Debug: 1; Information: 2; Warning: 3;
    Error: 4; Critical: 5; None: 6;
  };

  export class HubConnectionBuilder {
    withUrl(url: string, options?: { accessTokenFactory?: () => string | Promise<string> }): this;
    withAutomaticReconnect(retryDelays?: number[]): this;
    configureLogging(logLevel: any): this;
    build(): HubConnection;
  }

  export interface HubConnection {
    on(methodName: string, newMethod: (...args: any[]) => void): void;
    off(methodName: string): void;
    start(): Promise<void>;
    stop(): Promise<void>;
    invoke(methodName: string, ...args: any[]): Promise<any>;
    readonly state: HubConnectionState;
  }

  export enum HubConnectionState {
    Disconnected = 'Disconnected',
    Connecting = 'Connecting',
    Connected = 'Connected',
    Disconnecting = 'Disconnecting',
    Reconnecting = 'Reconnecting'
  }
}

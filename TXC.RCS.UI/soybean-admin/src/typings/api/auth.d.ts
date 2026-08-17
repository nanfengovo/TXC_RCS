declare namespace Api {
  /**
   * namespace Auth
   *
   * backend api module: "auth"
   */
  namespace Auth {
    interface LoginToken {
      access_token: string;
      refresh_token: string;
      token_type?: string;
      expires_in?: number;
      scope?: string;
    }

    interface UserInfo {
      userId: string;
      userName: string;
      roles: string[];
      buttons: string[];
    }

    interface ApplicationConfiguration {
      currentUser?: {
        isAuthenticated?: boolean;
        id?: string;
        userName?: string;
        roles?: string[];
      };
      auth?: {
        grantedPolicies?: Record<string, boolean>;
      };
    }
  }
}

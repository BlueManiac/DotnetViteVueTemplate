import { useLocalStorage } from "@vueuse/core";
import { api } from "/Features/api";

interface AccessTokenResponse {
    accessToken?: string;
    refreshToken?: string;
    expiresIn?: number;
    tokenType?: string;
    date?: number;
}

const loginResponse = useLocalStorage<AccessTokenResponse>('auth-login', {});

export const isLoggedIn = computed(() => loginResponse.value?.accessToken && Date.now() < loginResponse.value.date + loginResponse.value.expiresIn * 1000);
export const accessToken = computed(() => loginResponse.value?.accessToken)

const refresh = async () => {
    try {
        loginResponse.value = await api.post<AccessTokenResponse>('/auth/refresh', { refreshToken: loginResponse.value.refreshToken });
        loginResponse.value.date = Date.now();
    }
    catch {
        loginResponse.value = {};
        throw new Error('Could not refresh token');
    }
}

let refreshTimeout: NodeJS.Timeout;

const startBackgroundRefresh = () => {
    if (refreshTimeout) {
        clearTimeout(refreshTimeout);
    }

    if (!loginResponse.value?.refreshToken || !isLoggedIn.value) {
        loginResponse.value = {};
        return;
    }

    const refreshDelay = loginResponse.value.expiresIn * 1000 - (Date.now() - loginResponse.value.date) + 5000;

    refreshTimeout = setTimeout(async () => {
        if (isLoggedIn.value) {
            await refresh();

            startBackgroundRefresh()
        }
    }, refreshDelay);
}

export const login = async (username: string, password: string) => {
    loginResponse.value = await api.post<AccessTokenResponse>('/auth/login', { username, password });
    loginResponse.value.date = Date.now();

    // Spawn a background task to get updated login based on refresh token
    startBackgroundRefresh()
}

export const logout = async () => {
    loginResponse.value = {};
}

startBackgroundRefresh();
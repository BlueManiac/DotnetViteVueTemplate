import { computedAsync } from "@vueuse/core"
import { accessToken } from "./AuthenticationService"
import { api } from "../api"

const user = ref<string>(null)
export const userName = computedAsync(async () => {
    if (!accessToken.value) {
        return null
    }

    if (!user.value) {
        await getUserName()
    }

    return user.value
}, null, { lazy: true })

const getUserName = async () => {
    user.value = await api.get<string>('/auth/user')
}
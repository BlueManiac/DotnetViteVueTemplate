import { computedAsync } from "@vueuse/core"
import { isLoggedIn } from "./AuthenticationService"
import { api } from "../api"

type UserResponse = {
    name: string
}

const user = ref<UserResponse>(null)

const getUser = async () => {
    user.value = await api.get<UserResponse>('/auth/user')
}

export const userName = computedAsync(async () => {
    if (!isLoggedIn.value) {
        return null
    }

    if (!user.value) {
        await getUser()
    }

    return user.value.name
}, null, { lazy: true })
<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue' // ✅ 新增 onUnmounted
import { ElMessage } from 'element-plus'
import PackagingConfig from './components/PackagingConfig.vue'
import LoginView from './components/LoginView.vue'
import { authFetch } from './utils/request' // ✅ 引入封装的 fetch

const isLoggedIn = ref(false)
const currentUser = ref('')

onMounted(() => {
  checkLoginStatus()
  // ✅ 注册全局 401 监听器
  window.addEventListener('auth:unauthorized', handleUnauthorized)
})

onUnmounted(() => {
  // ✅ 组件卸载时移除监听
  window.removeEventListener('auth:unauthorized', handleUnauthorized)
})

// 被动登出处理函数
const handleUnauthorized = () => {
  handleLogout(true) // true 表示是被动过期
}

// 检查本地登录状态及有效性
const checkLoginStatus = () => {
  const token = localStorage.getItem('token')
  const savedUser = localStorage.getItem('username')
  const expireStr = localStorage.getItem('token_expire')

  if (token && savedUser) {
    if (expireStr) {
      const expireTime = new Date(expireStr).getTime()
      const now = new Date().getTime()
      if (now >= expireTime) {
        handleLogout(true)
        return
      }
    }
    isLoggedIn.value = true
    currentUser.value = savedUser
  } else {
    // 数据不完整，清理残留
    localStorage.removeItem('token')
    localStorage.removeItem('username')
    localStorage.removeItem('token_expire')
    isLoggedIn.value = false
  }
}

const onLoginSuccess = (username: string) => {
  currentUser.value = username
  isLoggedIn.value = true
}

/**
 * 处理登出逻辑
 * @param isSessionExpired - true: 被动过期(401/超时); false: 主动点击退出
 */
const handleLogout = async (isSessionExpired = false) => {
  const token = localStorage.getItem('token')

  // 仅在主动退出时尝试调用后端
  if (token && !isSessionExpired) {
    try {
      // ✅ 使用 authFetch 替代 fetch (保持一致性)
      await authFetch('/api/auth/logout', {
        method: 'POST'
      })
    } catch (error) {
      console.warn('后端注销接口调用失败，仅清理本地缓存', error)
    }
  }

  // 核心：清理本地存储
  localStorage.removeItem('token')
  localStorage.removeItem('username')
  localStorage.removeItem('token_expire')

  // 重置状态
  isLoggedIn.value = false
  currentUser.value = ''

  // 根据情况提示用户
  if (isSessionExpired) {
    // 避免重复弹窗（可选优化）
    ElMessage.warning('登录会话已失效，请重新登录')
  } else {
    ElMessage.success('已安全退出')
  }
}
</script>

<template>
  <transition name="fade" mode="out-in">
    <LoginView
        v-if="!isLoggedIn"
        @login-success="onLoginSuccess"
    />

    <PackagingConfig
        v-else
        :username="currentUser"
        @logout="() => handleLogout(false)"
    />
  </transition>
</template>

<style>
body {
  margin: 0;
  padding: 0;
  background-color: #f8fafc;
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
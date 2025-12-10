<template>
  <div class="login-container">
    <div class="login-card">
      <div class="brand-header">
        <div class="logo-box">📦</div>
        <h1 class="app-name">Packaging Studio <span class="pro-tag">PRO</span></h1>
        <p class="subtitle">企业级智能包装设计工作流</p>
      </div>

      <el-form
          ref="formRef"
          :model="loginForm"
          :rules="rules"
          class="login-form"
          size="large"
          @keyup.enter="handleLogin"
      >
        <el-form-item prop="username">
          <el-input
              v-model="loginForm.username"
              placeholder="账号"
              class="custom-input"
          >
            <template #prefix><el-icon class="input-icon"><User /></el-icon></template>
          </el-input>
        </el-form-item>

        <el-form-item prop="password">
          <el-input
              v-model="loginForm.password"
              type="password"
              placeholder="密码"
              show-password
              class="custom-input"
          >
            <template #prefix><el-icon class="input-icon"><Lock /></el-icon></template>
          </el-input>
        </el-form-item>

        <el-button
            type="primary"
            class="login-btn"
            :loading="isLoading"
            @click="handleLogin"
        >
          Sign In / 登录
        </el-button>
      </el-form>

      <div class="footer-text">
        &copy; 2025 Packaging Tech. All rights reserved.
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { User, Lock } from '@element-plus/icons-vue'
import { useLogin } from '../logic/useLogin' // 引入逻辑

// 定义事件
const emit = defineEmits(['login-success'])

// 使用组合式函数
const { formRef, isLoading, loginForm, rules, handleLogin } = useLogin(emit)
</script>

<style scoped lang="scss" src="../styles/LoginView.scss"></style>
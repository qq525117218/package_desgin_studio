# --- 第一阶段：构建 (Build Stage) ---
FROM node:22-alpine AS build-stage

# 设置工作目录
WORKDIR /app

# 复制 package.json 并安装依赖
# 核心优化：使用淘宝/阿里云镜像源加速 npm install
COPY package.json package-lock.json ./
RUN npm config set registry https://registry.npmmirror.com && \
    npm install

# 复制源代码并构建
COPY . .
# TypeScript 检查 + Vite 构建
RUN npm run build

# --- 第二阶段：生产环境 (Production Stage) ---
FROM nginx:stable-alpine as production-stage

# 复制构建产物到 Nginx 目录
COPY --from=build-stage /app/dist /usr/share/nginx/html

# 这一步我们在外部挂载配置，这里只需要暴露端口
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

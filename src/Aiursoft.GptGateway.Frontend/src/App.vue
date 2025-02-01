<template>
  <div class="dialog-container" ref="scrollContainer">
    <div
      v-for="(message, index) in dialogue.messages"
      :key="index"
      :class="['message', message.role === 'user' ? 'user-message' : 'assistant-message']"
    >
      <el-icon v-if="message.loading" class="is-loading" :size="20" style="margin: 1rem 0">
        <Loading />
      </el-icon>
      <div v-else v-html="message.content"></div>
    </div>
    <el-button class="reset-button" type="danger" @click="reset" icon="delete" plain circle></el-button>
  </div>

  <div class="answer-container">
    <div class="area-input">
      <el-form @submit.prevent @keyup.ctrl.enter="onSubmit" :model="formdata" label-width="120px" style="width: 100%">
        <el-input
          v-model="formdata.question"
          placeholder="请输入问题"
          type="textarea"
          :autosize="{ minRows: 4, maxRows: 8 }"
          resize="none"
          autofocus="autofocus"
          clearable
          size="large"
        >
          <template #append>
            <el-button :icon="Search" :loading="loading" :disabled="loading" @click="onSubmit" />
          </template>
        </el-input>
      </el-form>
    </div>
    <div class="button-group">
      <el-tooltip content="Ctrl+Enter" placement="top">
        <el-button @click="onSubmit" type="primary">发送</el-button>
      </el-tooltip>
    </div>
  </div>

  <el-space class="references">
    <el-link href="https://www.aiursoft.cn/" target="_blank">Home</el-link>
    <el-link href="https://gitlab.aiursoft.cn/aiursoft/gptgateway" target="_blank">Source</el-link>
    <el-tooltip :content="version" placement="top">
      <el-link href="">Commit</el-link>
    </el-tooltip>
    <el-link href="https://github.com/deepseek-ai/DeepSeek-R1" target="_blank">About</el-link>
  </el-space>
</template>

<script setup>
import { ref, reactive, onMounted, nextTick } from 'vue';
import { Search, Loading } from '@element-plus/icons-vue';
import { versionData } from './version.js';
import { auto as followSystemColorScheme } from 'darkreader';

const version = ref("");
const loading = ref(false);
const scrollContainer = ref(null);

const formdata = reactive({
  question: "",
});

const dialogue = reactive({
  messages: []
});

const onSubmit = () => {
  if (!formdata.question.trim()) return;
  getResult();
};

const reset = () => {
  formdata.question = "";
  dialogue.messages = [];
};

const getResult = async () => {
  // 插入用户消息
  dialogue.messages.push({
    role: 'user',
    content: formdata.question.trim()
  });
  // 清空输入框
  formdata.question = "";

  // 插入助手的占位消息（loading状态）
  const pendingMessage = reactive({
    role: 'assistant',
    content: '',
    loading: true
  });
  dialogue.messages.push(pendingMessage);

  await nextTick();
  scrollToBottom();

  // 构造完整对话历史（过滤掉正在 loading 的消息）
  const conversation = dialogue.messages
    .filter(msg => !msg.loading)
    .map(msg => ({ role: msg.role, content: msg.content }));

  loading.value = true;
  try {
    const response = await fetch('/v1/chat/completions', {
      method: 'POST',
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        messages: conversation
      })
    });
    const data = await response.json();
    // API 返回的数据中，最新消息在 data.message
    pendingMessage.content = data.message.content;
  } catch (error) {
    pendingMessage.content = error.toString();
  } finally {
    pendingMessage.loading = false;
    loading.value = false;
    await nextTick();
    scrollToBottom();
  }
};

const scrollToBottom = async () => {
  await nextTick();
  if (scrollContainer.value) {
    scrollContainer.value.scrollTo({
      top: scrollContainer.value.scrollHeight,
      behavior: 'smooth'
    });
  }
};

onMounted(() => {
  followSystemColorScheme();
  version.value = versionData.gitCommitId;
});
</script>

<style lang="less">
html,
body {
  margin: 0;
  height: 100%;
  overflow: hidden;
}

#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  padding: 0;
  height: 100vh;
  display: flex;
  flex-direction: column;
  margin: 0 auto;
  max-width: 768px;
  padding: 2rem 0 0;
  box-sizing: border-box;
}

.dialog-container {
  flex: 1;
  border: 1px solid #ccc;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 10px;
  box-sizing: border-box;
  padding: 2rem;
  position: relative;
}

.reset-button {
  position: absolute;
  right: 0.5rem;
  top: 0.5rem;
}

.message {
  padding: 1rem;
  border-radius: 5px;
  text-align: left;
  max-width: 80%;
  word-break: break-word;
}

/* 助手消息在左侧 */
.assistant-message {
  background-color: #f0f0f0;
  align-self: flex-start;
}

/* 用户消息在右侧 */
.user-message {
  background-color: #e6f7ff;
  align-self: flex-end;
}

.answer-container {
  border: 1px solid #ccc;
  border-top: none;
  position: relative;
  padding: 1rem;
}

.button-group {
  position: absolute;
  display: flex;
  flex-wrap: nowrap;
  right: 1.5rem;
  bottom: 1.5rem;
}

.area-input {
  width: 100%;
  display: flex;
  margin-right: 1rem;
}

.references {
  width: 768px;
  justify-content: space-around;
  margin: 2rem auto;
}

@media screen and (max-width: 768px) {
  #app {
    width: 100vw;
    padding: 0;
    border-top: none;
    border-left: none;
    border-right: none;
  }

  .dialog-container {
    width: 100%;
    border: none;
    border-bottom: 1px solid #ccc;
  }

  .answer-container {
    border: none;
    border-bottom: 1px solid #ccc;
  }

  .references {
    width: 100%;
    margin: 1rem auto;
  }
}
</style>

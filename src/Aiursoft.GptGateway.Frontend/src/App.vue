<template>
  <div class="dialog-container" ref="scrollContainer">
    <div
      v-for="(message, index) in dialogue.messages"
      :key="index"
      :class="message.isUser ? 'user-message' : 'bot-message'"
      class="message"
    >
      <el-icon v-if="message.loading" class="is-loading" :size="20" style="margin: 1rem 0"><Loading /></el-icon>
      <div v-else v-html="$filters.renderContent(message.content)"></div>
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
    <div class="button-group" fill>
      <el-tooltip :content="`Ctrl+Enter`" placement="top">
        <el-button @click="onSubmit" type="primary">发送</el-button>
      </el-tooltip>
    </div>
  </div>
  <el-space class="references">
    <el-link href="https://www.aiursoft.cn/" target="_blank">Home</el-link>
    <el-link href="https://gitlab.aiursoft.cn/aiursoft/glm-ui" target="_blank">Source </el-link>
    <el-tooltip :content="version" placement="top"><el-link href="">Commit</el-link></el-tooltip>
    <el-link href="https://huggingface.co/THUDM/chatglm-6b" target="_blank">About </el-link>
  </el-space>
</template>

<script setup>
import { ref, reactive, onMounted, nextTick } from "vue";
import { Search, House } from "@element-plus/icons-vue";
import { versionData } from "./version.js";
import { auto as followSystemColorScheme } from "darkreader";
const version = ref("");
const loading = ref(false);
const scrollContainer = ref(null);
const formdata = reactive({
  question: "",
});
const dialogue = reactive({
  messages: [],
});
const onSubmit = () => {
  getResult();
};
const reset = () => {
  formdata.question = "";
  dialogue.messages = [];
};
const getResult = async () => {
  const result = dialogue.messages.reduce((acc, curr, idx) => {
    if (idx % 2 === 0) {
      //当索引值是偶数时，创建一个新的子数组
      acc.push([curr.content]);
    } else {
      //当索引值是奇数时，将当前元素加到上一个子数组
      acc[acc.length - 1].push(curr.content);
    }
    return acc;
  }, []);
  await nextTick();
  dialogue.messages.push({ content: formdata.question, isUser: true });
  const respMessage = reactive({ loading: true, isUser: false });
  dialogue.messages.push(respMessage);
  scrollToBottom();
  try {
    const question = formdata.question;
    formdata.question = "";
    const resp = await fetch("https://glm.aiursoft.cn", {
      method: "post",
      body: JSON.stringify({
        prompt: question,
        history: result,
      }),
    });
    const data = await resp.json();
    console.log(data);
    respMessage.content = data.response;
  } catch (error) {
    respMessage.content = error;
  } finally {
    respMessage.loading = false;
  }
};
const scrollToBottom = async () => {
  await nextTick();
  if (scrollContainer.value) {
    scrollContainer.value.scrollTo({
      top: scrollContainer.value.scrollHeight,
      behavior: "smooth",
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
  padding: 0 1rem;
  border-radius: 5px;
  text-align: left;
  max-width: 80%;
}

.user-message {
  background-color: #e6f7ff;
  align-self: flex-start;
}

.bot-message {
  background-color: #f0f0f0;
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

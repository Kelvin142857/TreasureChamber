<template>
  <div>
    <h4 class="font-bold mb-3">系统设置</h4>
    <Message v-if="saved" severity="success" :closable="true" class="mb-3">设置已保存。</Message>

    <div class="grid">
      <div class="col-12 lg:col-6">
        <Card>
          <template #title>二维码访问地址</template>
          <template #content>
            <p class="text-muted small">
              客户手机扫码后访问的地址，需填<b>展厅电脑的局域网地址</b>（手机与电脑连接同一 WiFi）。
              填错或留空时二维码将无法打开详情页。
            </p>
            <div class="d-flex gap-2">
              <InputText v-model="baseUrl" class="flex-1" placeholder="例如：http://192.168.1.100:5000" />
              <Button label="保存" icon="pi pi-save" @click="save" :loading="saving" />
            </div>
            <div class="mt-3 small">
              <div class="text-muted mb-1">当前电脑可用局域网地址（供参考，选本机所在网段）：</div>
              <div v-for="ip in lanIps" :key="ip" class="font-monospace">http://{{ ip }}:5000</div>
              <div class="text-muted mt-2">当前访问地址：<span class="font-monospace">{{ requestBase }}</span></div>
            </div>
          </template>
        </Card>
      </div>

      <div class="col-12 lg:col-6">
        <Card>
          <template #title>使用说明</template>
          <template #content>
            <ul class="small mb-0" style="line-height:1.9">
              <li>本系统纯本地运行，不联网，数据保存在 <code>App_Data/treasure.db</code>（SQLite）。</li>
              <li>启动后默认监听 <code>http://0.0.0.0:5000</code>，同一局域网内的手机可访问。</li>
              <li>首次启动已内置示例产品数据，可直接体验浏览与二维码功能。</li>
              <li>建议首次运行后到「批量导入」上传正式产品目录。</li>
              <li>备份：直接复制 <code>App_Data</code> 文件夹即可完整备份数据。</li>
            </ul>
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useToast } from 'primevue/usetoast'
import api from '../api'

const toast = useToast()
const baseUrl = ref('')
const requestBase = ref('')
const lanIps = ref([])
const saved = ref(false)
const saving = ref(false)

onMounted(async () => {
  const { data } = await api.get('/settings')
  baseUrl.value = data.baseUrl
  requestBase.value = data.requestBase
  lanIps.value = data.lanIps
})

async function save() {
  saving.value = true
  try {
    await api.put('/settings', { baseUrl: baseUrl.value })
    saved.value = true
    toast.add({ severity: 'success', summary: '已保存', life: 2000 })
    setTimeout(() => (saved.value = false), 3000)
  } finally {
    saving.value = false
  }
}
</script>
